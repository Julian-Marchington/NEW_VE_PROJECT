using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using Ubiq.Messaging;
using Ubiq.Geometry;
using Avatar = Ubiq.Avatars.Avatar;

namespace Ubiq
{
    /// <summary>
    /// Captures and networks only the head input.
    /// </summary>
    public class HeadAvatar : MonoBehaviour
    {
        [Tooltip("The Avatar to use as the source of input. If null, will try to find an Avatar among parents at start.")]
        [SerializeField] private Avatar avatar;

        [Serializable]
        public class PoseUpdateEvent : UnityEvent<InputVar<Pose>> { }
        public PoseUpdateEvent OnHeadUpdate;

        [Serializable]
        private struct State
        {
            public Pose head;
        }

        private State[] state = new State[1];
        private NetworkContext context;
        private Transform networkSceneRoot;
        private float lastTransmitTime;

        protected void Start()
        {
            if (!avatar)
            {
                avatar = GetComponentInParent<Avatar>();
                if (!avatar)
                {
                    Debug.LogWarning("No Avatar could be found among parents. This script will be disabled.");
                    enabled = false;
                    return;
                }
            }

            context = NetworkScene.Register(this, NetworkId.Create(avatar.NetworkId, nameof(HeadAvatar)));
            networkSceneRoot = context.Scene.transform;
            lastTransmitTime = Time.time;
        }

        private void Update()
        {
            if (!avatar.IsLocal)
            {
                return;
            }

            // Update state from input, only head data is captured.
            state[0] = avatar.input.TryGet(out IHeadAndHandsInput src)
                ? new State { head = ToNetwork(src.head) }
                : new State { head = ToNetwork(InputVar<Pose>.invalid) };

            // Send state if it's time
            if ((Time.time - lastTransmitTime) > (1f / avatar.UpdateRate))
            {
                lastTransmitTime = Time.time;
                Send();
            }

            // Update any listeners with the new head pose.
            OnStateChange();
        }

        private Pose ToNetwork(InputVar<Pose> input)
        {
            return input.valid
                ? Transforms.ToLocal(input.value, networkSceneRoot)
                : GetInvalidPose();
        }

        private InputVar<Pose> FromNetwork(Pose net)
        {
            return !IsInvalid(net)
                ? new InputVar<Pose>(Transforms.ToWorld(net, networkSceneRoot))
                : InputVar<Pose>.invalid;
        }

        private void Send()
        {
            var transformBytes = MemoryMarshal.AsBytes(new ReadOnlySpan<State>(state));
            var message = ReferenceCountedSceneGraphMessage.Rent(transformBytes.Length);
            transformBytes.CopyTo(new Span<byte>(message.bytes, message.start, message.length));
            context.Send(message);
        }

        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            MemoryMarshal.Cast<byte, State>(
                new ReadOnlySpan<byte>(message.bytes, message.start, message.length))
                .CopyTo(new Span<State>(state));
            OnStateChange();
        }

        // Invoke the head update event with the new pose.
        private void OnStateChange()
        {
            OnHeadUpdate.Invoke(FromNetwork(state[0].head));
        }

        private static Pose GetInvalidPose()
        {
            return new Pose(new Vector3 { x = float.NaN }, Quaternion.identity);
        }

        private static bool IsInvalid(Pose p)
        {
            return float.IsNaN(p.position.x);
        }
    }
}