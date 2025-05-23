using UnityEngine;

namespace InTheDark
{
    public sealed class GameManager : MonoBehaviour
    {
        private static GameManager s_instance;

        public static GameManager Instance
        {
            get
            {
                return s_instance;
            }

            private set
            {
				s_instance = value;
			}
        }
    } 
}