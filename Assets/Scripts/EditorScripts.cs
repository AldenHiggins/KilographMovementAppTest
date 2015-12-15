#if UNITY_EDITOR

using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class EditorScripts
{
    ///////////////////////////////////////////////////////////////////////////
    ////////////////////////// GIF GENERATION   ///////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    //[MenuItem("Kilograph/Populate GIF")]
    //public static void populateGif()
    //{
    //    Get the current path that the user has navigated to in order to generate a skybox there
    //    string path = getSelectedPathOrFallback();

    //    Debug.Log("===========================");
    //    Debug.Log("Populating a GIF from textures at path:");
    //    Debug.Log(path);

    //    GameObject selectedObj = Selection.activeGameObject;
    //    Check to see if the user has selected an object
    //    if (selectedObj == null)
    //    {
    //        Debug.LogError("You need to select a gif to populate in the scene!");
    //        return;
    //    }
    //    Check to see if that object is an animated gif
    //    AnimatedGIF gif = selectedObj.GetComponent<AnimatedGIF>();
    //    if (gif == null)
    //    {
    //        Debug.LogError("The selected object: " + selectedObj.name + " doesn't have a gif script!");
    //        return;
    //    }

    //    Get the names of all the files in the current directory
    //    string[] files = getFiles(path);

    //    List<Texture2D> textures = returnTexturesFromFileList(files);

    //    if (textures.Count == 0)
    //    {
    //        Debug.LogError("The selected folder doesn't have any images in it!");
    //        return;
    //    }

    //    gif.frames = new Texture2D[textures.Count];

    //    Set the frames
    //    for (int textureIndex = 0; textureIndex < textures.Count; textureIndex++)
    //    {
    //        gif.frames[textureIndex] = textures[textureIndex];
    //    }

    //    Debug.Log("Done generating GIF");
    //    Debug.Log("===========================");
    //}

    static List<Texture2D> returnTexturesFromFileList(string[] files)
    {
        List<Texture2D> textures = new List<Texture2D>();

        for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
        {
            string filePath = files[fileIndex];
            if (filePath.Contains(".jpg") || filePath.Contains(".png"))
            {
                Texture2D texture = (Texture2D) AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D));
                if (texture != null)
                {
                    textures.Add(texture);
                }
            }
        }

        return textures;
    }



    ///////////////////////////////////////////////////////////////////////////
    //////////////////////// SKYBOX GENERATION   //////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    [MenuItem("Kilograph/Generate Skybox Object")]
    public static void generateSkyboxes()
    {
        // Get the current path that the user has navigated to in order to generate a skybox there
        string path = getSelectedPathOrFallback();

        Debug.Log("===========================");
        Debug.Log("Generating a new skybox from textures at path:");
        Debug.Log(path);

        // Get the names of all the files in the current directory
        string[] files = getFiles(path);
        // Generate a skybox if possible based on the files in this directory
        generateSkyboxFromFiles(path, files);

        Debug.Log("Done generating skybox");
        Debug.Log("===========================");
    }

    // Given the names of the files in the folder, generate a new skybox if possible
    static void generateSkyboxFromFiles(string path, string[] files)
    {
        // Check to see if files are found
        if (files.Length == 0)
        {
            Debug.LogError("Could not find files in: " + path);
            return;
        }

        // Each of the sides for the skybox
        string[] sides = { "Right", "Left", "Up", "Down", "Front", "Back" };

        // Generate all of the game objects required
        GameObject scene = new GameObject("Scene");
        GameObject leftImages = new GameObject("LeftCubeMap");
        leftImages.transform.localScale = new Vector3(5.0f, 5.0f, 5.0f);
        GameObject rightImages = new GameObject("RightCubeMap");
        rightImages.transform.localScale = new Vector3(5.0f, 5.0f, 5.0f);
        // Set the image groups to their respective eye layers
        leftImages.layer = 10;
        rightImages.layer = 11;

        // Set the image groups as children of the scene
        leftImages.transform.parent = scene.transform;
        rightImages.transform.parent = scene.transform;

        // Generate the faces of the cube
        generateCubeSides(sides, leftImages);
        generateCubeSides(sides, rightImages);

        // Keep track of the images found to delete an incomplete skybox if needed
        int imagesFound = 0;

        // Look through all of the files to find the images and attach them to the correct sides of the cubes
        for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
        {
            string fileName = files[fileIndex];
            // Skip non jpgs
            if (!fileName.Contains(".jpg"))
            {
                continue;
            }
            // Skip meta files
            if (fileName.Contains(".meta"))
            {
                continue;
            }

            GameObject sideParent = null;
            if (fileName.Contains("LEye"))
            {
                sideParent = leftImages;
            }
            else if (fileName.Contains("REye"))
            {
                sideParent = rightImages;
            }
            else
            {
                Debug.LogError("Skipping incorrectly formatted picture: " + fileName);
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(fileName);

            for (int sideIndex = 0; sideIndex < sides.Length; sideIndex++)
            {
                if (fileName.Contains(sides[sideIndex]))
                {
                    Material cubeMaterial = new Material(Shader.Find("Unlit/Texture"));
                    cubeMaterial.mainTexture = texture;

                    // Add the material to the assets folder
                    AssetDatabase.CreateAsset(cubeMaterial, path + "/" + sideParent.name + sides[sideIndex] + ".mat");

                    sideParent.transform.GetChild(sideIndex).gameObject.GetComponent<Renderer>().material = cubeMaterial;
                    imagesFound++;
                    break;
                }
            }
        }

        // Check to see if the correct amount of images were found
        if (imagesFound != 12)
        {
            Debug.LogError("Found " + imagesFound + " images with the right names instead of the required 12!");
            GameObject.DestroyImmediate(scene);
        }
    }

    // Helper function to create the six sides of the cube in the correct position for each parentSide (left and right)
    static void generateCubeSides(string[] sides, GameObject parentSide)
    {
        Vector3[] positions =
        {
            // Right
            new Vector3(5.0f, 0.0f, 0.0f),
            // Left
            new Vector3(-5.0f, 0.0f, 0.0f),
            // Up
            new Vector3(0.0f, 5.0f, 0.0f),
            // Down
            new Vector3(0.0f, -5.0f, 0.0f),
            // Front
            new Vector3(0.0f, 0.0f, 5.0f),
            // Back
            new Vector3(0.0f, 0.0f, -5.0f),
        };

        Quaternion[] orientations =
        {
            // Right
            Quaternion.Euler(0.0f, 90.0f, 0.0f),
            // Left
            Quaternion.Euler(0.0f, 270.0f, 0.0f),
            // Up
            Quaternion.Euler(270.0f, 180.0f, 0.0f),
            // Down
            Quaternion.Euler(90.0f, 180.0f, 0.0f),
            // Front
            Quaternion.Euler(0.0f, 0.0f, 0.0f),
            // Back
            Quaternion.Euler(0.0f, 180.0f, 0.0f),
        };

        for (int sideIndex = 0; sideIndex < sides.Length; sideIndex++)
        {
            GameObject side = GameObject.CreatePrimitive(PrimitiveType.Quad);
            side.name = sides[sideIndex];
            side.transform.parent = parentSide.transform;
            side.layer = parentSide.layer;
            side.transform.localScale = new Vector3(10.01f, 10.01f, 1.0f);
            side.transform.localPosition = positions[sideIndex];
            side.transform.localRotation = orientations[sideIndex];
        }
    }


    ///////////////////////////////////////////////////////////////////////////
    ////////////////////////  HELPER FUNCTIONS   //////////////////////////////
    ///////////////////////////////////////////////////////////////////////////
    // Get the current path that the user has navigated to
    public static string getSelectedPathOrFallback()
    {
        string path = "Assets";

        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }

    // Find all of the names of the files within path then attempt to generate a skybox out of them
    static string[] getFiles(string path)
    {
        string[] files = null;
        try
        {
            files = Directory.GetFiles(path);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }

        return files;
    }
}

#endif // UNITY_EDITOR
