using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ACT
{
    public interface IACTEditor
    {
        bool ShowOriginalGizmos { get; set; }
        bool ShowStoredGizmos { get; set; }
        bool ShowPrevGizmos { get; set; }
        bool ShowHeadGizmos { get; set; }
        bool ShowAvatarSkeleton { get; set; }
        bool ShowModelSkeleton { get; set; }
        bool IsAvatarInspectorActive { get; set; }
        float HandleSize { get; set; }
        Data Data { get; set; }
        //Settings Settings { get; set; }

        //Skeleton StoredSkeleton { get; set; }
        //bool Repaint { get; set; }
    }
}
