using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Eve_Ship_ID.Models
{
    public class ShipQuiz
    {
        public ShipQuiz()
        {
            score = string.Empty;
        }

        public string ShipName { get; set;}
        public List<String> ShipTypeOptions { get; set; }
        public string score { get; set; }
        //public int  nbrCorrect { get; private set; }
        //public int nbrIncorrect { get; private set; }
        private int nbrCorrect;
        private int nbrIncorrect;

        public string ParseScore()
        {
 
            //foreach (char c in score)
            //{
            //    if (c == '1') correct++;
            //    if (c == '0') incorrect++;
            //}

            if (score.Length == 0) //no score yet
            {
                return string.Empty;
            }

            CalculateScore();

            return "Correct: " + nbrCorrect.ToString() + " | Incorrect: " + nbrIncorrect.ToString();
            //return string.Empty;
        }

        private void CalculateScore()
        {
            var correct = 0;
            var incorrect = 0;

            var fullScore = score.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            if (fullScore.Length % 2 != 0)  //score should be even: one id and one correct/incorrect for each question.
            {
                System.Diagnostics.Debug.Print("Score Error. score string is: " + score);
                nbrCorrect = 0;
                nbrIncorrect = 0;
            }
            else
            {
                for (var s = 0; s < fullScore.Length; s++)
                {
                    if ((s % 2 != 0) && (fullScore[s] == "1"))
                    {
                        correct++;
                    }
                    else
                        if ((s % 2 != 0) && (fullScore[s] == "0"))
                        {
                            incorrect++;
                        }
                    //else  //add shipid to a hashtable or something
                    //if (s % 2 != 0)
                    //{

                    //}

                }
            }

            nbrCorrect = correct;
            nbrIncorrect = incorrect;
        }

        public int getNbrCorrect()
        {
            CalculateScore();
            return nbrCorrect; 
        }

        public int getNbrIncorrect()
        {
            CalculateScore();
            return nbrIncorrect;
        }

    }
}