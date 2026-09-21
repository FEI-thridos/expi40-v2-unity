using UnityEngine;

namespace Assets.Scripts.Environment
{
    /// <summary>
    /// Closes the application when invoked.
    /// </summary>
    public class CloseApplication : MonoBehaviour
    {
        /// <summary>
        /// Closes the application.
        /// </summary>
        public void OnClick()
        {
            Application.Quit();
        }
    }
}
