using System;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit
{
    public enum ProfileAction
    {
        UserTimeline,
        Favorites,
        Following,
        Followers
    }
    interface IProfileViewer
    {
        ProfileAction selectedAction{ get;}
        string selectedUser{ get;}
    }
}
