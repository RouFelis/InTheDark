using System.Collections;
using UnityEngine;
using TMPro;



    public class ScrambleTextEffect : MonoBehaviour
    {
        public float scrambleSpeed = 0.05f;
        public float revealInterval = 0.1f;

        private string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789가나다라마바사아자차카타파하!@#$%^&*";


        public IEnumerator ScrambleCoroutine(string Text, TMP_Text textComponent)
        {
            int revealCount = 0;
            int totalLength = Text.Length;

            while (revealCount < totalLength)
            {
                string currentText = "";

                for (int i = 0; i < totalLength; i++)
                {
                    if (i < revealCount)
                    {
                        currentText += Text[i];
                    }
                    else
                    {
                        currentText += characters[Random.Range(0, characters.Length)];
                    }
                }

                textComponent.text = currentText;
                yield return new WaitForSeconds(scrambleSpeed);

                // 특정 간격마다 하나씩 정답으로 고정
                if (Time.time % revealInterval < scrambleSpeed)
                {
                    revealCount++;
                }
            }

            textComponent.text = Text;
        }
    }
