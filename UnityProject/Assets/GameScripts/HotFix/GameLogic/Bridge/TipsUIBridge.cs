using System.Collections;
using System.Collections.Generic;
using GameLogic;
using TEngine;
using UnityEngine;

public class TipsUIBridge : MonoBehaviour
{
    private void AnimationFinished()
    {
        GameEvent.Get<ITipsUI>().AnimationFinished();
    }
}
