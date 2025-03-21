using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;

public class CanvasPainter : MonoBehaviour
{
    [Header("Texture Settings")]
    public int textureWidth = 512;
    public int textureHeight = 512;
    public Color backgroundColor = Color.white;

    [Header("Drawing Settings")]
    public int brushSize = 5;
    public Color drawColor = Color.black; // Default drawing color

    private Texture2D drawingTexture;
    private Renderer rend;
    private Mouse mouse;

    void Start()
    {
        // Get the Renderer component on the canvas (Quad)
        rend = GetComponent<Renderer>();
        mouse = Mouse.current;

        // Create a new blank texture with RGBA32 format
        drawingTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        // Fill the texture with the background color
        Color[] pixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }
        drawingTexture.SetPixels(pixels);
        drawingTexture.Apply();

        // Set the material's main texture to the newly created texture
        rend.material.mainTexture = drawingTexture;
    }

    void Update()
    {
        if (mouse == null) return;

        // For testing, use mouse input. Replace with VR input as needed.
        if (mouse.leftButton.isPressed)
        {
            Vector2 mousePosition = mouse.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the ray hit this canvas
                if (hit.collider.gameObject == gameObject)
                {
                    DrawAtPoint(hit.textureCoord);
                }
            }
        }
    }

    // Draw on the texture at the given UV coordinate
    void DrawAtPoint(Vector2 uv)
    {
        int x = (int)(uv.x * textureWidth);
        int y = (int)(uv.y * textureHeight);

        // Loop through a square of pixels based on brushSize
        for (int i = -brushSize; i < brushSize; i++)
        {
            for (int j = -brushSize; j < brushSize; j++)
            {
                int pixelX = x + i;
                int pixelY = y + j;

                // Check that we are within bounds of the texture
                if (pixelX >= 0 && pixelX < textureWidth && pixelY >= 0 && pixelY < textureHeight)
                {
                    drawingTexture.SetPixel(pixelX, pixelY, drawColor);
                }
            }
        }
        drawingTexture.Apply(); // Update the texture with the new pixels
    }

    // Public method to change the drawing color (can be called from UI buttons)
    public void SetDrawingColor(Color newColor)
    {
        drawColor = newColor;
    }

    // Optional: Save the drawing to a PNG file
    public void SaveDrawing()
    {
        byte[] bytes = drawingTexture.EncodeToPNG();
        string path = Application.persistentDataPath + "/MyDrawing.png";
        File.WriteAllBytes(path, bytes);
        Debug.Log("Drawing saved at: " + path);
    }
}