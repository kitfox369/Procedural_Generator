using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProceduralMap3D : MonoBehaviour
{
    private int _width = 10;
    private int _depth = 10;
    private int _heightScale = 5;
    private float _noiseScale = 0.1f;
    private int _seed;
    public GameObject cubePrefab;
    public GameObject blockPrefab;
    public GameObject waterPrefab;

    public Transform mapParentTransfrom;

    public TMP_InputField _seedInputField;
    public TMP_InputField _widthInputField;
    public TMP_InputField _depthInputField;
    public TMP_InputField heightInputField;
    public Button generateButton;

    private List<Vector3> _blockPositions = new List<Vector3>();

    private Vector3 _mapCenterPosition;

    [SerializeField] private Camera _camera;

    // Start is called before the first frame update
    void Start()
    {
        generateButton.onClick.AddListener(GenerateMapFromInput);
    }


    void GenerateMapFromInput()
    {
        _width = int.Parse(_widthInputField.text);
        _depth = int.Parse(_depthInputField.text);
        _heightScale = int.Parse(heightInputField.text);

        _seed = int.Parse(_seedInputField.text);

        // 기존 맵 오브젝트 제거 (새로 생성하기 위해)
        foreach (Transform child in mapParentTransfrom)
        {
            Destroy(child.gameObject);
        }

        Generate3DMap();
        _mapCenterPosition = CalculateCenter();
        ResetCameraPosition();
    }

    void Generate3DMap()
    {
        _blockPositions.Clear();

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _depth; z++)
            {
                float noiseValue = Mathf.PerlinNoise((x+_seed) * _noiseScale, (z+_seed) * _noiseScale);
                int yHeight = Mathf.RoundToInt(noiseValue * _heightScale);

                for (int y = 0; y < yHeight; y++)
                {
                    Vector3 position = new Vector3(x, y, z);

                    // 경계 처리: 맵 가장자리에 벽 블록 추가
                    if (y == 0)
                    {
                        Instantiate(waterPrefab, position, Quaternion.identity, mapParentTransfrom);
                    }
                    else if (x == 0 || z == 0 || x == _width - 1 || z == _depth - 1)
                    {
                        Instantiate(blockPrefab, position, Quaternion.identity, mapParentTransfrom);
                    }
                    else
                    {
                        Instantiate(cubePrefab, position, Quaternion.identity, mapParentTransfrom);
                    }

                    _blockPositions.Add(position);
                }
            }
        }
    }

    Vector3 CalculateCenter()
    {
        if (_blockPositions.Count == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;

        foreach (Vector3 pos in _blockPositions) 
            sum += pos;

        return sum / _blockPositions.Count;
    }

    void ResetCameraPosition()
    {
        _camera.transform.position = new Vector3(_mapCenterPosition.x, 10f, _mapCenterPosition.z-_depth);
        //_camera.transform.LookAt(_mapCenterPosition);

      

        //calculate map diagonal
        float diagonal = Mathf.Sqrt(_width * _width + _depth * _depth);

        float cameraDistance = diagonal / (2 * Mathf.Tan(Mathf.Deg2Rad) * _camera.fieldOfView / 2f);
        _camera.transform.position =
            new Vector3(_mapCenterPosition.x, cameraDistance, _mapCenterPosition.z - cameraDistance);


        Vector3 direction = _mapCenterPosition - _camera.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        _camera.transform.rotation = targetRotation;

    }
}
