using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponMulticollectible : Multicollectible
{
    [Space]
    public GameObject iconFrame;
    [Space]
    public GameObject[] assortment;

    protected HumanballCell[] humanballFilledCells;

    protected List<HumanController> armedHumans;
    protected List<HumanController> unarmedHumans;

    protected HumanController human;

    protected GameObject weaponInstance;

    protected int weaponIndex;

    public int WeaponID => weaponIndex;

    public virtual void Initialize(int weaponIndex, int elementsCount = 1)
    {
        base.Initialize(elementsCount);

        this.weaponIndex = weaponIndex;

        assortment[weaponIndex - 1].SetActive(true);

        for (int i = 0; i < elementsCount; i++)
        {
            weaponInstance = Instantiate(assortment[weaponIndex - 1], transform.position + Random.insideUnitSphere, Quaternion.Euler(Random.insideUnitSphere), transform);

            weaponInstance.transform.localScale /= 2f;

            weaponInstance.SetActive(false);

            elements[i] = new MulticollectibleElement(weaponInstance.transform, multicollectibleSettings.collectiblePullingSpeed, multicollectibleSettings.collectiblePullingDelay);
        }
    }

    protected override void ProcessCollecting()
    {
        for (int i = 0; i < elements.Length; i++)
        {
            if (elements[i].IsCollecting)
            {
                if (elements[i].Pull(PlayerController.Humanball.Transform))
                {
                    if (unarmedHumans.Count > 0)
                    {
                        PlayerController.Humanball.StickWeapon(unarmedHumans.CutRandom(), weaponIndex);
                    }
                    else if (armedHumans.Count > 0)
                    {
                        human = armedHumans.CutRandom();

                        if (human.Weapon.WeaponID < weaponIndex)
                        {
                            PlayerController.Humanball.StickWeapon(human, weaponIndex);
                        }
                    }

                    elements[i].Transform.gameObject.SetActive(false);
                }
            }
        }
    }

    protected override IEnumerator CollectingCoroutine()
    {
        humanballFilledCells = PlayerController.Humanball.Structure.FilledCells;

        armedHumans = new List<HumanController>();
        unarmedHumans = new List<HumanController>();

        for (int i = 0; i < humanballFilledCells.Length; i++)
        {
            (humanballFilledCells[i].Human.Weapon.WeaponID > 0 ? armedHumans : unarmedHumans).Add(humanballFilledCells[i].Human);
        }

        assortment[0].transform.parent.gameObject.SetActive(false);

        BreakCapsules(PlayerController.Humanball.Velocity);
        DropElements(transform.position, PlayerController.Humanball.Velocity);

        yield return base.CollectingCoroutine();
    }
}

public class WeaponMulticollectibleInfo
{
    public int id;
    public int count;

    public WeaponMulticollectibleInfo(int id, int count)
    {
        this.id = id;
        this.count = count;
    }
}