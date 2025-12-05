using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CampLifeManager : MonoBehaviour
{
    public static CampLifeManager Instance;

    public List<ActivityPoint> allPoints = new List<ActivityPoint>();

    void Awake()
    {
        Instance = this;
        // Sahnedeki tüm noktaları otomatik bul
        allPoints = FindObjectsOfType<ActivityPoint>().ToList();
    }

    // Rastgele boş bir nokta bul (Belirli bir türde)
    public ActivityPoint GetFreePoint(ActivityPoint.PointType type)
    {
        // İstenen türde ve boş olan noktaları filtrele
        var validPoints = allPoints.Where(p => p.activityType == type && !p.isOccupied).ToList();

        if (validPoints.Count > 0)
        {
            // Rastgele birini seç
            return validPoints[Random.Range(0, validPoints.Count)];
        }
        return null; // Yer yok
    }
}