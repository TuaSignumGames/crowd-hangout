using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspendedCollectible : Collectible
{
    [Space]
    public ConnectedRope rope;

    protected float swingPeriod;
    protected float swingPeriodOffset;

    public override void Initialize(BlockPair blockPair, float placementFactor = 0.5f)
    {
        base.Initialize(blockPair, placementFactor);

        UpdatePlacement();

        swingPeriod = Random.Range(rope.swingPeriodRange.x, rope.swingPeriodRange.y);
        swingPeriodOffset = Random.Range(0, swingPeriod);
    }

    private void FixedUpdate()
    {
        if (!rope.IsConnected)
        {
            rope.UpdateBouncing();
        }
    }

    private void LateUpdate()
    {
        if (rope.IsConnected)
        {
            rope.swingContainer.localEulerAngles = new Vector3(0, 0, Mathf.Sin(6.28f * (swingPeriodOffset + Time.timeSinceLevelLoad) / swingPeriod) * rope.swingAmplitude);
        }
    }

    public override void Collect()
    {
        base.Collect();

        rope.Release();
    }

    public override void UpdatePlacement()
    {
        transform.SetParent(null);

        base.UpdatePlacement();

        rope.Connect(transform, blockPair.ceilBlock.transform.position);
    }

    [System.Serializable]
    public class ConnectedRope
    {
        public GameObject ropeContainer;
        [Space]
        public Transform lineTransform;
        [Space]
        public Transform originTransform;
        public Transform endTransform;
        [Space]
        public Transform swingContainer;
        [Space]
        public float swingAmplitude;
        public Vector2 swingPeriodRange;
        [Space]
        public SpringData elasticitySettings;

        private Transform collectibleTransform;

        private SpringEvaluator springEvaluator;

        private float springValue;

        private bool isConnected;

        public bool IsConnected => isConnected;

        public void Connect(Transform collectibleTransform, Vector2 point)
        {
            this.collectibleTransform = collectibleTransform;

            ropeContainer.transform.SetParent(collectibleTransform.parent);

            swingContainer.position = point;
            endTransform.position = point;

            lineTransform.position = originTransform.position;
            lineTransform.localScale = new Vector3(1f, (endTransform.position.y - originTransform.position.y), 1f);

            lineTransform.SetParent(swingContainer);

            collectibleTransform.SetParent(swingContainer);

            isConnected = true;
        }

        public void Release()
        {
            springEvaluator = new SpringEvaluator(elasticitySettings, 0.2f, 1f, 0.02f);

            isConnected = false;
        }

        public void UpdateBouncing()
        {
            springEvaluator.Update(ref springValue);

            swingContainer.localScale = new Vector3(1f, springValue, 1f);
        }
    }
}
