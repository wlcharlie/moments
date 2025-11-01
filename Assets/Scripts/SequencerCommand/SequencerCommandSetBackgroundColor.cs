using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    public class SequencerCommandSetBackgroundColor : SequencerCommand
    {

        public void Awake()
        {
            // Get hex color code from parameter
            // Usage: SetBackgroundColor(#RRGGBB) or SetBackgroundColor(#RRGGBBAA)
            // Example: SetBackgroundColor(#00000012)

            string hexColor = GetParameter(0);

            if (string.IsNullOrEmpty(hexColor))
            {
                Debug.LogWarning("SetBackgroundColor: No color parameter provided.");
                Stop();
                return;
            }

            // Ensure hex color starts with #
            if (!hexColor.StartsWith("#"))
            {
                hexColor = "#" + hexColor;
            }

            Color backgroundColor;
            if (ColorUtility.TryParseHtmlString(hexColor, out backgroundColor))
            {
                // Find the Background GameObject by tag
                GameObject backgroundObj = GameObject.FindGameObjectWithTag("Background");
                if (backgroundObj != null)
                {
                    SpriteRenderer spriteRenderer = backgroundObj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = backgroundColor;
                    }
                    else
                    {
                        Debug.LogWarning("SetBackgroundColor: SpriteRenderer not found on Background object.");
                    }
                }
                else
                {
                    Debug.LogWarning("SetBackgroundColor: Background object with tag 'Background' not found.");
                }
            }
            else
            {
                Debug.LogWarning($"SetBackgroundColor: Invalid hex color '{hexColor}'.");
            }

            Stop();
        }

        public void Update()
        {
            // This command completes immediately in Awake(), so Update() is not needed.
        }

        public void OnDestroy()
        {
            // No cleanup needed for this command.
        }

    }

}


/**/
