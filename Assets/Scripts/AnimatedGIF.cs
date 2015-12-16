using UnityEngine;
using System.Collections;

public class AnimatedGIF : MonoBehaviour
{
    public Texture2D[] frames;
    public int FramesPerSecond;
    public bool loopFromEndOfGif;
    private bool hasLooped = false;
    private int loopIndex;
    private int previousIndex;

    // Swap the frames of the gif
	void Update ()
    {
        int index = (int)((Time.time * ((float)FramesPerSecond))) % frames.Length;

        // If looping normally just bound the index by the amount of frames and start from frame zero
        if (loopFromEndOfGif)
        {
            if (index != previousIndex)
            {
                if (previousIndex == frames.Length - 1)
                {
                    if (loopIndex != 0)
                    {
                        hasLooped = true;
                    }
                    else
                    {
                        hasLooped = false;
                    }
                }
            }

            previousIndex = index;

            if (hasLooped)
            {
                loopIndex = frames.Length - 1;
                loopIndex -= index;
                index = loopIndex;
            }
            else
            {
                loopIndex = index;
            }
        }

        GetComponent<Renderer>().material.mainTexture = frames[index];
	}
}
