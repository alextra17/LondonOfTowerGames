using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TowerOfLondon : MonoBehaviour
{
    // Класс уровеня: количество колец, максимальное количество ходов, начальная конфигурация.
    // При помощи их создаётся новый уровень.
    [Serializable]
    public class Level
    {
        public int numRings;
        public int maxMoves;
        public List<int> startPegConfig;
    }

    public List<Level> levels = new List<Level>();
    public GameObject ringPrefab;
    public Transform[] pegs; 
    public float ringSpacing = 0.5f;
    public float liftHeight = 2f;  
    public Color[] ringColors;

    
    public Text movesText;
    public Text maxMovesText;
    public Text levelText;
    public Button restartButton;
    public Button nextLevelButton;
    public GameObject winPanel;
    public GameObject failPanel;
    public Text resultTimeText;
    public Text resultMovesText;
    public InputField playerNameInputField;

    private List<GameObject>[] ringStacks; 
    private GameObject selectedRing;       
    private Vector3 selectedRingOriginalPos;  
    private bool isDragging = false;          
    private int currentLevelIndex = 0;      
    private int currentMoves = 0;          
    private DateTime levelStartTime;      

    private string dataFilePath;            

    private float dragDelay = 0.2f;   
    private bool canDrag = false;      

   
    public AudioClip clickUpSound;
    public AudioClip clickDownSound;
    public AudioClip loseSound;
    public AudioClip winSound;
    public AudioClip deniaSound;
    private AudioSource audioSource;

    private bool canMoveRings = true;
    void Start()
    {
        ringStacks = new List<GameObject>[3];
        for (int i = 0; i < 3; i++)
        {
            ringStacks[i] = new List<GameObject>();
        }

        restartButton.onClick.AddListener(RestartLevel);
        nextLevelButton.onClick.AddListener(LoadNextLevel);

        dataFilePath = Application.persistentDataPath + "/tower_of_london_data.txt";
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        LoadLevel(currentLevelIndex);
    }


    void Update()
    {
        if (!canMoveRings) return;

        if (Input.GetMouseButtonDown(0))
        {
            TrySelectRing();
        }

        if (selectedRing != null && canDrag)
        {
            if (Input.GetMouseButton(0))
            {
                if (!isDragging)
                {
                    selectedRing.transform.position = selectedRingOriginalPos + Vector3.up * liftHeight;
                    isDragging = true;
                    PlaySound(clickUpSound);
                }
                else
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Plane plane = new Plane(Vector3.up, selectedRingOriginalPos + Vector3.up * liftHeight);
                    float distance;
                    if (plane.Raycast(ray, out distance))
                    {
                        selectedRing.transform.position = ray.GetPoint(distance);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            DropRing();
        }
    }


    void TrySelectRing()
    {
        if (!canMoveRings) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Ring"))
        {
            GameObject hitRing = hit.collider.gameObject;
            int pegIndex = -1;
            for (int i = 0; i < pegs.Length; i++)
            {
                if (ringStacks[i].Contains(hitRing))
                {
                    pegIndex = i;
                    break;
                }
            }

            if (pegIndex == -1) return;

            if (ringStacks[pegIndex].Count > 0 && ringStacks[pegIndex][ringStacks[pegIndex].Count - 1] == hitRing)
            {
                selectedRing = hitRing;
                selectedRingOriginalPos = selectedRing.transform.position;
                canDrag = true;
            }
            else
            {
                PlaySound(deniaSound);
                Debug.Log("Это не верхнее кольцо.");
                StartCoroutine(WrongSelectionFeedBack(hit.collider.gameObject));
            }
        }
    }


    // Дроп кольца на столбик, если он свободен/подходит по условию
    void DropRing()
    {
        if (selectedRing == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Peg"))
            {
                Transform targetPeg = hit.collider.transform;
                int pegIndex = Array.IndexOf(pegs, targetPeg);

                if (IsValidMove(pegIndex))
                {
                    MoveRing(pegIndex);
                }
                else
                {
                    selectedRing.transform.position = selectedRingOriginalPos;
                    PlaySound(clickDownSound);
                }
            }
            else
            {
                selectedRing.transform.position = selectedRingOriginalPos;
                PlaySound(clickDownSound);
            }
        }
        else
        {
            selectedRing.transform.position = selectedRingOriginalPos;
            PlaySound(clickDownSound);
        }

        selectedRing = null;
        isDragging = false;
        canDrag = false;
    }

    
    bool IsValidMove(int targetPegIndex)
    {
        return ringStacks[targetPegIndex].Count == 0 || selectedRing.transform.localScale.x < ringStacks[targetPegIndex][ringStacks[targetPegIndex].Count - 1].transform.localScale.x;
    }

    
    void MoveRing(int targetPegIndex)
    {
        int oldPegIndex = -1;
        for (int i = 0; i < pegs.Length; i++)
        {
            if (ringStacks[i].Contains(selectedRing))
            {
                oldPegIndex = i;
                break;
            }
        }
        if (oldPegIndex == -1) return;

        ringStacks[oldPegIndex].Remove(selectedRing);
        ringStacks[targetPegIndex].Add(selectedRing);
        Vector3 targetPosition = pegs[targetPegIndex].position + Vector3.up * ringSpacing * (ringStacks[targetPegIndex].Count - 1);
        selectedRing.transform.position = targetPosition;

        PlaySound(clickDownSound);

        currentMoves++;
        movesText.text = "Ходов: " + currentMoves;

        if (CheckWinCondition())
        {
            WinLevel();
        }
        else if (currentMoves >= levels[currentLevelIndex].maxMoves)
        {
            ShowFailPanel();
        }
    }

    // Проверка условия для победы
    bool CheckWinCondition()
    {
        int targetPegIndex = 2;

        if (ringStacks[targetPegIndex].Count != levels[currentLevelIndex].numRings)
        {
            return false;
        }
        for (int i = 0; i < ringStacks[targetPegIndex].Count - 1; i++)
        {
            if (ringStacks[targetPegIndex][i].transform.localScale.x <= ringStacks[targetPegIndex][i + 1].transform.localScale.x)
            {
                return false;
            }
        }
        return true;
    }


    void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError("Сломанный индекс уровня");
            return;
        }

        canMoveRings = true;
        levelText.text = $"Уровень {levelIndex + 1}";
        ClearRings();
        Level currentLevel = levels[levelIndex];
        maxMovesText.text = "Макс. ходов: " + currentLevel.maxMoves;
        currentMoves = 0;
        movesText.text = "Ходов: " + currentMoves;

        for (int i = 0; i < currentLevel.numRings; i++)
        {
            GameObject newRing = Instantiate(ringPrefab);
            newRing.transform.localScale = ringPrefab.transform.localScale + new Vector3(i * 0.4f, 0, i * 0.4f);
            newRing.GetComponent<Renderer>().material.color = ringColors[i % ringColors.Length];

            int pegIndex = currentLevel.startPegConfig[i];
            ringStacks[pegIndex].Add(newRing);
            Vector3 ringPos = pegs[pegIndex].position + Vector3.up * ringSpacing * (ringStacks[pegIndex].Count - 1);
            newRing.transform.position = ringPos;
            newRing.name = "Ring_" + i + "_" + pegIndex;
        }

        levelStartTime = DateTime.Now;
        nextLevelButton.gameObject.SetActive(false);
        winPanel.SetActive(false);
        failPanel.SetActive(false);
        restartButton.gameObject.SetActive(true);
    }


    void ClearRings()
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (GameObject ring in ringStacks[i])
            {
                Destroy(ring);
            }
            ringStacks[i].Clear();
        }
    }

    // Победа
    void WinLevel()
    {
        winPanel.SetActive(true);
        TimeSpan timeTaken = DateTime.Now - levelStartTime;
        resultTimeText.text = "Время: " + timeTaken.ToString(@"mm\:ss\.fff");
        resultMovesText.text = "Ходов: " + currentMoves;

        nextLevelButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(false);
        canMoveRings = false;

        PlaySound(winSound);
    }


    // Сохранение результата игрока.
    void SaveResult(string result)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(dataFilePath, true))
            {
                sw.WriteLine(result);
            }
            Debug.Log($"Результаты сохранены в: {dataFilePath}") ;
        }
        catch (Exception e)
        {
            Debug.LogError("Не удалось сохранить результаты: " + e.Message);
        }
    }

    
    void RestartLevel()
    {
        LoadLevel(currentLevelIndex);
    }

   
    void LoadNextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex >= levels.Count)
        {
            currentLevelIndex = 0;
            Debug.Log("Все уровни пройдены.  Новая игра.");
        }
        LoadLevel(currentLevelIndex);
    }

   
    private void ShowFailPanel()
    {
        failPanel.SetActive(true);
        nextLevelButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(true);

        PlaySound(loseSound);
    }

    
    IEnumerator WrongSelectionFeedBack(GameObject selected)
    {
        Color originColor = selected.GetComponent<Renderer>().material.color;
        for (int i = 0; i < 3; i++)
        {
            selected.GetComponent<Renderer>().material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            selected.GetComponent<Renderer>().material.color = originColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    [ContextMenu("Добавить уровень")]
    public void AddLevelEditor()
    {
        Level level = new Level() { numRings = 3, maxMoves = 7, startPegConfig = new List<int> { 0, 0, 0 } };
        levels.Add(level);
    }

   
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}