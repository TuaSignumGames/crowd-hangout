using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspendedCollectible : Collectible
{
    public ConnectedRopeSettings ropeSettings;

    protected bool isRopeConnected;

    public override void Initialize(BlockPair blockPair, float placementFactor = 0.5f)
    {
        base.Initialize(blockPair, placementFactor);

        ropeSettings.Connect(transform, blockPair.ceilBlock.transform.position);
    }

    private void LateUpdate()
    {
        if (isRopeConnected)
        {
            // TODO Swinging 
        }
    }

    public override void Collect()
    {

    }

    [System.Serializable]
    public class ConnectedRopeSettings
    {
        public GameObject ropeContainer;
        [Space]
        public Transform lineTransform;
        public Transform endTransform;
        [Space]
        public Transform swingContainer;
        [Space]
        public float swingAmplitude;
        public float swingPeriod;

        public void Connect(Transform collectibleTransform, Vector2 point)
        {
            ropeContainer.transform.SetParent(collectibleTransform.parent);

            swingContainer.position = point;
            endTransform.position = point;

            lineTransform.localScale = new Vector3(1f, (endTransform.position.y - lineTransform.position.y), 1f);

            lineTransform.SetParent(swingContainer);

            collectibleTransform.SetParent(swingContainer);
        }
    }
}
