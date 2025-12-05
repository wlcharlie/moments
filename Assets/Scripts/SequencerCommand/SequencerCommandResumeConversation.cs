using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to resume conversation based on current game mode.
    /// Usage: ResumeConversation()
    ///
    /// - StoryMode: Returns to MainStoryScene and resumes saved conversation
    /// - EventMode: Returns to ExploreScene
    /// </summary>
    public class SequencerCommandResumeConversation : SequencerCommand
    {
        public void Awake()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeConversation();
            }
            else
            {
                Debug.LogError("ResumeConversation: GameManager.Instance is null.");
            }

            Stop();
        }
    }
}
