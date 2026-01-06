using System;
using System.Collections.Generic;
using Dreamteck.Splines;
using Dreamteck.Splines.Examples;
using UnityEngine;

public partial class Boggy
{
    [Serializable]
    public struct BoggyTypeData
    {
        public BoggyType boggyType;
        public GameObject boggyObj;

        public void ActivateNewBoggy(BoggyType boggyType)
        {
            boggyObj.SetActive(boggyType == this.boggyType);
        }
    }
}
