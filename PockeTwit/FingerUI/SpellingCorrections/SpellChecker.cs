using System;

using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using PockeTwit.OtherServices.GoogleSpell;

namespace PockeTwit.FingerUI.SpellingCorrections
{
    class SpellChecker
    {
        
        public delegate void delDone();
        public event delDone Done;
        private TextBox _textBoxToCheck;
        private ISpellingCorrector _corrector;
        private SpellCorrection currentSC;
        private List<SpellCorrection> _corrections = null;
        private string _originalText;
        public SpellChecker(TextBox TextBoxToCheck)
        {
            _textBoxToCheck = TextBoxToCheck;
            if(DetectDevice.DeviceType==DeviceType.Professional)
            {
                _corrector = new PocketPCCorrecter();
            }
            else
            {
                _corrector = new SmartPhoneCorrector();
            }
            _corrector.AppliesTo = _textBoxToCheck;
            _corrector.ItemSelected+=_corrector_ItemSelected;
        }


        public void CheckSpelling()
        {
            /*string textToCheck = _textBoxToCheck.Text;
            _originalText = textToCheck;
            SpellRequest request = new SpellRequest(textToCheck, false, false, false);
            SpellResult result = SpellCheck.Check(request);
            
            if (result.Corrections == null || result.Corrections.Length <= 0)
            {
                Done();
                return;
            }

            _corrections = new List<SpellCorrection>(result.Corrections);
            _originalText = _textBoxToCheck.Text;

            CorrectNext();*/
            Done();
            return;
        }


        private void CorrectNext()
        {
            if (_corrections.Count > 0)
            {
                CorrectSpelling(_corrections[0]);
            }
            else
            {
                _textBoxToCheck.SelectionStart = _textBoxToCheck.Text.Length;
                _textBoxToCheck.SelectionLength = 0;
                Done();
            }
        }
        private void CorrectSpelling(SpellCorrection spellCorrection)
        {
            try
            {
                //if the mispelled word is a username or hashtag, don't spell check it
                if (spellCorrection.Offset == 0 ||
                    _originalText.Substring(spellCorrection.Offset - 1, spellCorrection.Length + 1).StartsWith("@") ||
                    _originalText.Substring(spellCorrection.Offset - 1, spellCorrection.Length + 1).StartsWith("#"))
                {
                    return;
                }

                //if the mispelled word no longer exists, it was probably a duplicate
                if (_textBoxToCheck.Text.IndexOf(_originalText.Substring(spellCorrection.Offset, spellCorrection.Length)) < 0)
                {
                    return;
                }

                currentSC = spellCorrection;

                _textBoxToCheck.SelectionStart = _textBoxToCheck.Text.IndexOf(_originalText.Substring(currentSC.Offset, currentSC.Length));

                //don't select "misspelled" text that starts with @ or #
                while (_textBoxToCheck.Text.Substring(_textBoxToCheck.SelectionStart - 1, 1).Equals("@") ||
                       _textBoxToCheck.Text.Substring(_textBoxToCheck.SelectionStart - 1, 1).Equals("#"))
                {
                    MessageBox.Show("Moving selection pointer again!");
                    _textBoxToCheck.SelectionStart = _textBoxToCheck.Text.IndexOf(_originalText.Substring(currentSC.Offset, currentSC.Length), _textBoxToCheck.SelectionStart + 1);
                }

                _textBoxToCheck.SelectionLength = currentSC.Length;

                string[] vals = spellCorrection.Value.Split('\t');
                _corrector.Suggestions = vals;
                _corrector.Display();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Spell check error: " + ex.Message);
            }
        }

        void _corrector_ItemSelected(string text)
        {
            _textBoxToCheck.Text = _textBoxToCheck.Text.Replace(_originalText.Substring(currentSC.Offset, currentSC.Length), text);
            _corrections.RemoveAt(0);
            CorrectNext();
        }

    }
}
