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
        public int  nbrCorrect { get; private set; }
        public int nbrIncorrect { get; private set; }

        public string ParseScore()
        {
            var correct = 0;
            var incorrect = 0;
            foreach (char c in score)
            {
                if (c == '1') correct++;
                if (c == '0') incorrect++;
            }
            
            nbrCorrect = correct;
            nbrIncorrect = incorrect;

            return "Correct: " + correct.ToString() + " | Incorrect: " + incorrect.ToString();
        }

    }
}