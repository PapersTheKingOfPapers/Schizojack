using UnityEngine;

public class TVSchizojackTurnScript : MonoBehaviour
{
    public GameObject[] tvScreens;
    public GameObject[] tvScreensYTObject;
    public GameObject[] tvScreensWObject;

    public Material tvScreenYTMat;
    public Material tvScreenWMat;

    [SerializeField] SchizojackBackend SB;

    private bool _active = false;

    private int _prevCurrentTurn = -1;

    public void ToggleActiveScreens()
    {
        /*for(int i = 0; i < SB._actors.Count; i++)
        {
            tvScreens[i].SetActive(true);
        }*/
        _active = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(_prevCurrentTurn != SB._currentTurn && _active)
        {
            for(int i = 0; i< tvScreensYTObject.Length; i++)
            {
                tvScreens[i].GetComponent<Renderer>().materials[1] = i == SB._currentTurn ? tvScreenYTMat : tvScreenWMat;
                tvScreensYTObject[i].SetActive(i == SB._currentTurn);
                tvScreensWObject[i].SetActive(i != SB._currentTurn);
            }
            _prevCurrentTurn = SB._currentTurn;
        }
    }
}
