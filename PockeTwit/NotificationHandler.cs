﻿using System;
using Microsoft.Win32;
using Microsoft.WindowsCE.Forms;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit
{
    class NotificationHandler
    {
        private int NewMessagesCount = 0;
        private int NewFriendsCount = 0;
        private int CurrentSpinner = 0;
        public delegate void delNotificationClicked();
        public event delNotificationClicked MessagesNotificationClicked;
        private christec.windowsce.forms.NotificationWithSoftKeys MessagesBubbler;
        [Flags]
        public enum Options
        {
            Sound = 1,
            Vibrate = 2,
            Flash = 4,
            Message = 8
        }
        public struct NotificationInfo
        {
            public string GUID;
            public Options Options;
            public string Sound;
        }

        public static Options FriendsOptions;
        public static Options MessagesOptions;
        public static bool NotifyOfFriends
        {
            get
            {
                CheckSettings();
                return (FriendsOptions != 0);
            }
        }

        public static bool NotifyOfMessages
        {
            get
            {
                CheckSettings();
                return (MessagesOptions != 0);
            }
        }

        public const string FriendsTweets = "{DF293090-5095-49ce-A626-AE6D6629437F}";
        public const string MessageTweets = "{B4D35E62-A83F-4add-B421-F7FC28E14310}";

        public static NotificationInfo Friends = new NotificationInfo();
        public static NotificationInfo Messages = new NotificationInfo();

        public NotificationHandler()
        {
            MessagesBubbler = new christec.windowsce.forms.NotificationWithSoftKeys();
            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                //MessagesBubbler = new christec.windowsce.forms.NotificationWithSoftKeys();
                MessagesBubbler.Icon = Properties.Resources.MyIco;
                MessagesBubbler.RightSoftKey = new christec.windowsce.forms.NotificationSoftKey(christec.windowsce.forms.SoftKeyType.Dismiss, "Dismiss");
                MessagesBubbler.LeftSoftKey = new christec.windowsce.forms.NotificationSoftKey(christec.windowsce.forms.SoftKeyType.StayOpen, "Show");
                MessagesBubbler.LeftSoftKeyClick += new EventHandler(MessagesBubbler_LeftSoftKeyClick);
                MessagesBubbler.SpinnerClick += new christec.windowsce.forms.SpinnerClickEventHandler(MessagesBubbler_SpinnerClick);
                MessagesBubbler.Silent = true;   
            }
        }

        void MessagesBubbler_SpinnerClick(object sender, christec.windowsce.forms.SpinnerClickEventArgs e)
        {
            if (e.Forward)
            {
                CurrentSpinner = 1;
            }
            else
            {
                CurrentSpinner = 0;
            }

            ShowMessageForSpinner();
        }

        private void ShowMessageForSpinner()
        {
            switch (CurrentSpinner)
            {
                case 0:
                    MessagesBubbler.Text = GetMessagesText();
                    MessagesBubbler.Caption = "PockeTwit\t1 of 2";
                    break;
                case 1:
                    MessagesBubbler.Text = GetFriendsText();
                    MessagesBubbler.Caption = "PockeTwit\t2 of 2";
                    break;

            }
        }

        void MessagesBubbler_LeftSoftKeyClick(object sender, EventArgs e)
        {
            NewMessagesCount = 0;
            NewFriendsCount = 0;
            MessagesBubbler.Visible = false;
            if (MessagesNotificationClicked != null)
            {
                MessagesNotificationClicked();
            }
        }


        void t_StopVibrate_Tick(object sender, EventArgs e)
        {
            VibrateStop();
        }

        private static void CheckSettings()
        {
            RegistryKey FriendsKey = Registry.CurrentUser.OpenSubKey("\\ControlPanel\\Notifications\\" + FriendsTweets);
            RegistryKey MessageKey = Registry.CurrentUser.OpenSubKey("\\ControlPanel\\Notifications\\" + MessageTweets);
            if (FriendsKey == null)
            {
                return;
            }
            if (FriendsKey.GetValue("Options") != null)
            {
                FriendsOptions = (Options)FriendsKey.GetValue("Options");
            }
            if (MessageKey.GetValue("Options") != null)
            {
                MessagesOptions = (Options)MessageKey.GetValue("Options");
            }
        }
        public static void LoadSettings()
        {
            Friends.GUID = FriendsTweets;
            Messages.GUID = MessageTweets;
            RegistryKey FriendsKey = Registry.CurrentUser.OpenSubKey("\\ControlPanel\\Notifications\\" + FriendsTweets);
            RegistryKey MessageKey = Registry.CurrentUser.OpenSubKey("\\ControlPanel\\Notifications\\" + MessageTweets);
            if (FriendsKey != null)
            {
                Friends.Sound = (string)FriendsKey.GetValue("Wave");
                if (FriendsKey.GetValue("Options") != null)
                {
                    Friends.Options = (Options)FriendsKey.GetValue("Options");
                    FriendsOptions = Friends.Options;
                }
            }

            if (MessageKey != null)
            {
                Messages.Sound = (string)MessageKey.GetValue("Wave");

                if (MessageKey.GetValue("Options") != null)
                {
                    Messages.Options = (Options)MessageKey.GetValue("Options");
                    MessagesOptions = Messages.Options;
                }
            }
        }

        public static void SaveSettings(NotificationInfo InfoSet)
        {
            RegistryKey TheKey = Registry.CurrentUser.OpenSubKey("\\ControlPanel\\Notifications\\" + InfoSet.GUID, true);
            if (TheKey == null)
            {
                TheKey = Registry.CurrentUser.CreateSubKey("\\ControlPanel\\Notifications\\" + InfoSet.GUID);
            }
            TheKey.SetValue("Wave", InfoSet.Sound);
            
            TheKey.SetValue("Options", (int)InfoSet.Options);
            LoadSettings();
        }

        private void ShowNotifications()
        {
            if (GlobalEventHandler.isInForeground()) { return; }
            MessagesBubbler.Visible = false;
            if (NewFriendsCount > 0 && NewMessagesCount > 0)
            {
                MessagesBubbler.Spinners = true;
                MessagesBubbler.Caption = "PockeTwit\t1 of 2";
            }
            else
            {
                MessagesBubbler.Caption = "PockeTwit";
            }
            if (!MessagesBubbler.Visible)
            {
                if (NewMessagesCount > 0)
                {
                    MessagesBubbler.Text = GetMessagesText();
                }
                else
                {
                    MessagesBubbler.Text = GetFriendsText();
                }
                CurrentSpinner = 0;
                MessagesBubbler.Visible = true;
            }
            else
            {
                ShowMessageForSpinner();
            }
        }

        public void NewBoth(int MessagesCount, int FriendsCount)
        {
            MessagesBubbler.Visible = false;
            NewMessagesCount += MessagesCount;
            NewFriendsCount += FriendsCount;
            LoadSettings();
            NewFriendMessages(0);
            NewMessages(0);
            ShowNotifications();
        }
        public void NewFriendMessages(int Count)
        {
            MessagesBubbler.Visible = false;
            NewFriendsCount += Count;
            LoadSettings();
            
            if ((Friends.Options & Options.Vibrate) == Options.Vibrate)
            {
                VibrateStart();
                if ((Friends.Options & Options.Sound) == Options.Sound)
                {
                    Sound s = new Sound(Friends.Sound);
                    s.Play();
                }
                VibrateStop();
            }
            else if ((Friends.Options & Options.Sound) == Options.Sound)
            {
                Sound s = new Sound(Friends.Sound);
                s.Play();
            }
            if (Count > 0)
            {
                if ((Friends.Options & Options.Message) == Options.Message)
                {
                    ShowNotifications();
                }
            }
        }


        public void NewMessages(int Count)
        {
            NewMessagesCount += Count;
            LoadSettings();

            if ((Messages.Options & Options.Vibrate) == Options.Vibrate)
            {
                VibrateStart();
                if ((Messages.Options & Options.Sound) == Options.Sound)
                {
                    Sound s = new Sound(Messages.Sound);
                    s.Play();
                }
                VibrateStop();
            }
            else if ((Messages.Options & Options.Sound) == Options.Sound)
            {
                Sound s = new Sound(Messages.Sound);
                s.Play();
            }
            if (Count > 0)
            {
                if ((Messages.Options & Options.Message) == Options.Message)
                {
                    ShowNotifications();
                }
            }
        }

        private string GetFriendsText()
        {
            System.Text.StringBuilder HTMLString = new StringBuilder();
            HTMLString.Append("<html><body>");
            HTMLString.Append("New friend update(s) are available.");
            HTMLString.Append("</body></html>");
            return HTMLString.ToString();
        }
        private string GetMessagesText()
        {
            System.Text.StringBuilder HTMLString = new StringBuilder();
            HTMLString.Append("<html><body>");
            HTMLString.Append("New message(s) are available.");
            HTMLString.Append("</body></html>");

            return HTMLString.ToString();
        }


        #region VibrateCode
        private void VibrateStop()
        {
            vib.SetLedStatus(1, Led.LedState.Off);
        }
        
        void VibrateStart() 
        {
            vib.SetLedStatus(1, Led.LedState.On);
        }
        #endregion
        private Led vib = new Led();
    }
}