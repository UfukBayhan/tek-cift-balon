using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OddOrEvenGame : MonoBehaviour
{
    private StemGameManager stem;
    private OddOrEvenInfo info;

    private TextMeshProUGUI yonergeText;
    private GameObject WinSFX;
    private GameObject WinSFXFinal;
    private GameObject WrongSFX;
    private GameObject SoundFX;

    public OddOrEven targetType = OddOrEven.odd; // Başta TEK/ÇİFT seç
    public bool isRandom = false;
    public int goalCount = 3; // Kaç doğru seçim yapılmalı
    public Vector2 numberRange = new Vector2(0, 20);
    public float moveSpeed = 1.3f;
    public int levelIndex = 0; // Kaçıncı round
    private bool levelTransitioning = false; // re-entrancy guard

    [Header("Spawn Settings")]
    public GameObject objectParent;
    public GameObject childPrefab;
    public Transform[] spawnpoints;
    public float spawnTime = 2f;

    [Header("Runtime Values")]
    private int score = 0;
    private int remaining;
    private float nextSpawnTime = 0f;
    private int lastSpawnIndex = -1; // en son kullanılan spawnpoint

    [Header("Difficulty Curves (Test)")]
    public AnimationCurve goalCurve = AnimationCurve.Linear(0, 3, 50, 15);
    public AnimationCurve rangeCurve = AnimationCurve.Linear(0, 10, 50, 50);
    public AnimationCurve spawnCurve = AnimationCurve.Linear(0, 1.6f, 50, 1.0f);
    public AnimationCurve speedCurve = AnimationCurve.Linear(0, 2f, 50, 5f);

    void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        stem = FindAnyObjectByType<StemGameManager>();
        if (stem != null)
        {
            yonergeText = stem.yonergeText;
            WinSFX = stem.WinSFX;
            WrongSFX = stem.WrongSFX;
            WinSFXFinal = stem.WinSFXFinal;
            targetType = stem.targetType;
            isRandom = stem.isRandom;
            goalCount = stem.step;
            numberRange = stem.numberRangeXY;
            levelIndex = stem.levelIndex;
            objectParent = stem.trueObjects[0];
            childPrefab = stem.etcPrefab[0];
        }
        // Rastgele hedef seç (tek/çift)
        if (isRandom)
            targetType = (Random.Range(0, 2) == 0) ? OddOrEven.even : OddOrEven.odd;

        remaining = goalCount;
        UpdateYonergeText();

        // spawnpointleri doldur
        if (objectParent != null)
        {
            spawnpoints = new Transform[objectParent.transform.childCount];
            for (int i = 0; i < objectParent.transform.childCount; i++)
                spawnpoints[i] = objectParent.transform.GetChild(i);
        }
    }

    void Update()
    {
        if (!levelTransitioning && Time.time >= nextSpawnTime && spawnpoints.Length > 0)
        {
            SpawnObject();
            nextSpawnTime = Time.time + spawnTime;
        }
    }

    private void SpawnObject()
    {
        if (childPrefab == null || spawnpoints.Length == 0)
            return;

        int spawnIndex;
        do
        {
            spawnIndex = Random.Range(0, spawnpoints.Length);
        } while (spawnIndex == lastSpawnIndex && spawnpoints.Length > 1);

        lastSpawnIndex = spawnIndex;
        Transform spawnpoint = spawnpoints[spawnIndex];

        GameObject clone = Instantiate(
            childPrefab,
            spawnpoint.position,
            Quaternion.identity,
            objectParent.transform
        );

        int number = Random.Range((int)numberRange.x, (int)numberRange.y);
        clone.GetComponentInChildren<TextMeshProUGUI>().text = number.ToString();

        OddOrEven numberType = (number % 2 == 0) ? OddOrEven.even : OddOrEven.odd;

        info = clone.AddComponent<OddOrEvenInfo>();
        info.numberType = numberType;

        var movable = clone.GetComponent<MovableObject>();
        if (movable != null)
        {
            movable.speed = moveSpeed;
            Debug.Log("Speed Mode aktif");
        }

        Button btn = clone.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => CatchedObject(clone));
        }
    }

    public void CatchedObject(GameObject catched)
    {
        if (levelTransitioning || catched == null)
            return;

        OddOrEvenInfo info = catched.GetComponent<OddOrEvenInfo>();
        if (info == null)
            return;
        if (info.isCaught)
            return;
        info.isCaught = true;
        var btn = catched.GetComponent<Button>();
        if (btn)
            btn.interactable = false;
        Image balloonImg = catched.GetComponent<Image>();
        if (balloonImg != null)
            balloonImg.enabled = false;

        TextMeshProUGUI numberText = catched.GetComponentInChildren<TextMeshProUGUI>();
        if (numberText != null)
            numberText.enabled = false;
        Transform explodingChild = catched.transform.Find("BalloonExploding");
        if (explodingChild != null)
        {
            explodingChild.gameObject.SetActive(true);

            // animasyon süresini hesapla
            float delay = 0.25f;

            Destroy(catched, delay);
        }
        else
        {
            Destroy(catched);
        }
        stem.DestroyAllSFX();
        // doğru seçim
        if (info.numberType == targetType)
        {
            score++;
            remaining--;
            Instantiate(WinSFX).name = "WinSFX";

            CheckGameProgress();
        }
        else
        {
            score--;
            Instantiate(WrongSFX).name = "WrongSFX";
        }

        UpdateYonergeText();
    }

    void UpdateYonergeText()
    {
        yonergeText.text =
            remaining
            + " adet '"
            + (targetType == OddOrEven.odd ? "TEK" : "ÇİFT")
            + "' sayı yakala";
    }

    public void CheckGameProgress()
    {
        if (remaining <= 0 && !levelTransitioning)
        {
            TriggerFinale();
            levelTransitioning = true;
            StartCoroutine(ResetLevelTransitionWhenSfxEnds());
        }
    }

    private void TriggerFinale()
    {
        // önceki SFX sahnedeyse kaldır
        if (SoundFX != null)
        {
            Destroy(SoundFX);
            SoundFX = null;
        }

        SoundFX = Instantiate(WinSFXFinal);
        SoundFX.name = "WinSFXFinal";
    }

    private IEnumerator ResetLevelTransitionWhenSfxEnds()
    {
        if (SoundFX != null)
        {
            var audioSource = SoundFX.GetComponent<AudioSource>();

            if (audioSource != null && audioSource.isPlaying)
            {
                while (audioSource.isPlaying)
                {
                    yield return null;
                }
            }
        }
        if (SoundFX != null)
        {
            Destroy(SoundFX);
            SoundFX = null;
        }
        foreach (var old in objectParent.GetComponentsInChildren<OddOrEvenInfo>()) Destroy(old.gameObject);
        levelIndex++;
        stem.FinalAnswer();
        ApplyDifficultyWithGroup(levelIndex);

        if (isRandom)
            targetType = (Random.Range(0, 2) == 0) ? OddOrEven.even : OddOrEven.odd;

        remaining = goalCount;
        UpdateYonergeText();

        levelTransitioning = false;
    }

    private void ApplyDifficultyWithGroup(int index)
    {
        int clampedIndex = Mathf.Min(index, 100);

        int groupIndex = clampedIndex / 5;
        int groupStart = groupIndex * 5;
        int groupEnd = Mathf.Min(groupStart + 4, 100);
        float groupProgress = (clampedIndex % 5) / 4f;

        // Küçük dalgalar
        float waveGoal = (Mathf.Sin(clampedIndex * Mathf.PI / 18f) + 1f) * 0.5f;

        float baseGoal = groupIndex * 1f; // seviye ilerledikçe hafif artış
        float baseSpeed = groupIndex * 0.01f; // daha yumuşak artış (100'de +2.0 civarı)

        // Goal
        float goalMin = goalCurve.Evaluate(groupStart);
        float goalMax = goalCurve.Evaluate(groupEnd) + 4;
        goalCount = Mathf.RoundToInt(
            baseGoal + LerpWithWave(goalMin, goalMax, waveGoal, groupProgress)
        );

        // Range
        int rangeUpper = GetRangeUpperForLevel100(index);
        numberRange = new Vector2(0, rangeUpper);

        // Spawn
        spawnTime = GetSpawnForLevel100(index);

        // Speed (100’de 5.0’a clamp)
        float spd = GetSpeedForLevel100(index) + baseSpeed;
        moveSpeed = Mathf.Min(spd, 5.0f);

        // 100 sonrası sabitler
        if (index > 100)
        {
            spawnTime = 1.0f;
            moveSpeed = 5.0f;
            numberRange = new Vector2(0, 9999);
        }

        Debug.Log(
            $"[LEVEL {index}] Goal={goalCount}, Range={numberRange.y}, Spawn={spawnTime:F2}, Speed={moveSpeed:F2}"
        );
    }

    private float GetSpawnForLevel100(int level)
    {
        // 0→100: 2.00 → 1.00 (parçalı şekilde)
        if (level <= 20)
            return Mathf.Lerp(2.00f, 1.60f, level / 20f);
        if (level <= 40)
            return Mathf.Lerp(1.60f, 1.40f, (level - 20) / 20f);
        if (level <= 60)
            return Mathf.Lerp(1.40f, 1.25f, (level - 40) / 20f);
        if (level <= 80)
            return Mathf.Lerp(1.25f, 1.10f, (level - 60) / 20f);
        if (level <= 100)
            return Mathf.Lerp(1.10f, 1.00f, (level - 80) / 20f);
        return 1.00f;
    }

    private int GetRangeUpperForLevel100(int level)
    {
        float up;
        if (level <= 20)
            up = Mathf.Lerp(100f, 600f, level / 20f);
        else if (level <= 40)
            up = Mathf.Lerp(600f, 1200f, (level - 20) / 20f);
        else if (level <= 60)
            up = Mathf.Lerp(1200f, 4000f, (level - 40) / 20f);
        else if (level <= 80)
            up = Mathf.Lerp(4000f, 7000f, (level - 60) / 20f);
        else
            up = Mathf.Lerp(7000f, 9999f, (level - 80) / 20f);

        // Küçük dalga (%3 oynama)
        float wave = (Mathf.Sin(level * Mathf.PI / 12f) + 1f) * 0.5f;
        float wobble = Mathf.Lerp(-0.03f, 0.03f, wave);
        up *= (1f + wobble);

        return Mathf.Clamp(Mathf.RoundToInt(up), 10, 9999);
    }

    private float GetSpeedForLevel100(int level)
    {
        // 0 → 100: 1.5 → 5.0
        return Mathf.Lerp(1.5f, 5.0f, level / 100f);
    }

    private float LerpWithWave(float min, float max, float wave, float progress)
    {
        return Mathf.Lerp(min, max, wave * progress);
    }
}

[System.Serializable]
public enum OddOrEven
{
    odd,
    even,
}

public class OddOrEvenInfo : MonoBehaviour
{
    public OddOrEven numberType;
    public bool isCaught;
}


