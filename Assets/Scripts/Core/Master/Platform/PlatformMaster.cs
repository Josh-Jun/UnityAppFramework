using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace App.Core.Master
{
    public abstract partial class PlatformMaster
    {
        public abstract void OpenPhotoAlbum(Action<Texture2D> callback);
    }
}
