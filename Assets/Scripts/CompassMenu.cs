using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassMenu : MonoBehaviour
{
    [SerializeField] private DirectionalItem directionalItemPrefab;
    [SerializeField] private Button turnOnBtn;
    [SerializeField] private Button turnOffCompassBtn;
    [SerializeField] private GameObject directionalsObject;
    private bool isActive = false;

    private DirectionRotationCalculator DirectionRotationCalculator;

    private void Awake()
    {
        turnOnBtn.gameObject.SetActive(true);
        turnOffCompassBtn.gameObject.SetActive(false);
        directionalsObject.gameObject.SetActive(false);
        
        InitRotationCalculator();

        SetupToggleButton();

        InitDirections();
    }

    private void SetupToggleButton()
    {
        turnOnBtn.onClick.AddListener(() => { Show(true); });
        turnOffCompassBtn.onClick.AddListener(() => { Show(false); });
    }

    private void Show(bool iShow)
    {
        directionalsObject.gameObject.SetActive(iShow);
        turnOnBtn.gameObject.SetActive(!iShow);
        turnOffCompassBtn.gameObject.SetActive(iShow);
    }


    private void InitDirections()
    {
        List<Direction> directions = new()
        {
            Direction.East, // icon is look to this direction so this is default rotation
            Direction.South,
            Direction.North,
            Direction.West
        };
        for (int i = 0; i < directions.Count; i++)
        {
            var item = Instantiate(directionalItemPrefab, directionalsObject.transform);
            var itemRect = item.GetComponent<RectTransform>();
            var direction = directions[i];
            var anchor = direction.ToAnchor();

            item.Set(directions[i]);
            item.SetAnchor(itemRect, anchor);
            item.SetAnchor(item.Icon, anchor);

            DirectionRotationCalculator.SetZRotation(item.Icon, direction);
        }
    }

    private void InitRotationCalculator()
    {
        DirectionRotationCalculator = new();
        DirectionRotationCalculator.circleDirection = new()
        {
            Direction.North, // 0
            Direction.East, // 90
            Direction.South, // 180
            Direction.West // 270
        };
        DirectionRotationCalculator.Init();
    }

    private void Toggle()
    {
        isActive = !isActive;
        directionalsObject.gameObject.SetActive(isActive);
    }
}