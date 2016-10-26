using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using HoloToolkit.Unity;

/// <summary>
/// The SurfaceManager class allows applications to scan the environment for a specified amount of time 
/// and then process the Spatial Mapping Mesh (find planes, remove vertices) after that time has expired.
/// </summary>
public class PlaySpaceManager : Singleton<PlaySpaceManager>
{
    [Tooltip("When checked, the SurfaceObserver will stop running after a specified amount of time.")]
    public bool limitScanningByTime = true;

    [Tooltip("How much time (in seconds) that the SurfaceObserver will run after being started; used when 'Limit Scanning By Time' is checked.")]
    public float scanTime = 30.0f;

    [Tooltip("Material to use when rendering Spatial Mapping meshes while the observer is running.")]
    public Material defaultMaterial;

    [Tooltip("Optional Material to use when rendering Spatial Mapping meshes after the observer has been stopped.")]
    public Material secondaryMaterial;

    [Tooltip("Minimum number of floor planes required in order to exit scanning/processing mode.")]
    public uint minimumFloors = 1;

    [Tooltip("Minimum number of wall planes required in order to exit scanning/processing mode.")]
    public uint minimumWalls = 1;

    private bool meshesProcessed = false;

    private void Start()
    {
        SpatialMappingManager.Instance.SetSurfaceMaterial(defaultMaterial);
        SurfaceMeshesToPlanes.Instance.MakePlanesComplete += SurfaceMeshesToPlanes_MakePlanesComplete;
    }

    private void Update()
    {
        if (!meshesProcessed && limitScanningByTime)
        {
            if ((Time.time - SpatialMappingManager.Instance.StartTime) > scanTime)
            {
                if (SpatialMappingManager.Instance.IsObserverRunning())
                {
                    SpatialMappingManager.Instance.StopObserver();
                }
                
                CreatePlanes();
                meshesProcessed = true;
            }
        }
    }

    private void SurfaceMeshesToPlanes_MakePlanesComplete(object source, System.EventArgs args)
    {
        List<GameObject> horizontal = new List<GameObject>();
        List<GameObject> vertical = new List<GameObject>();
        
        horizontal = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Floor);
        vertical = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Wall);
        
        if (horizontal.Count >= minimumFloors && vertical.Count >= minimumWalls)
        {
            Debug.Log("Found sufficient floors. Stopping scan.");
            RemoveVertices(SurfaceMeshesToPlanes.Instance.ActivePlanes);
            SpatialMappingManager.Instance.SetSurfaceMaterial(secondaryMaterial);
        }
        else
        {
            Debug.Log("Floor not detected yet. Continuing scan...");
            SpatialMappingManager.Instance.StartObserver();
            meshesProcessed = false;
        }
    }

    private void CreatePlanes()
    {
        SurfaceMeshesToPlanes surfaceToPlanes = SurfaceMeshesToPlanes.Instance;
        if (surfaceToPlanes != null && surfaceToPlanes.enabled)
        {
            surfaceToPlanes.MakePlanes();
        }
    }
    
    private void RemoveVertices(IEnumerable<GameObject> boundingObjects)
    {
        RemoveSurfaceVertices removeVerts = RemoveSurfaceVertices.Instance;
        if (removeVerts != null && removeVerts.enabled)
        {
            removeVerts.RemoveSurfaceVerticesWithinBounds(boundingObjects);
        }
    }
    
    private void OnDestroy()
    {
        if (SurfaceMeshesToPlanes.Instance != null)
        {
            SurfaceMeshesToPlanes.Instance.MakePlanesComplete -= SurfaceMeshesToPlanes_MakePlanesComplete;
        }
    }
}