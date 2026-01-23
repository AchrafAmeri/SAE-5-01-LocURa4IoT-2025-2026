//using NUnit.Framework;
//using UnityEngine;
//#if UNITY_XR_ARFOUNDATION
//using UnityEngine.XR.ARFoundation;
//#endif

//[TestFixture]
//public class ToggleARTests
//{
//    private GameObject testObject;
//    private ToggleAR toggleAR;
//    private Camera testCamera;

//    [SetUp]
//    public void SetUp()
//    {
//        // 1. Création de l'objet qui porte le script
//        testObject = new GameObject("ToggleAR_Test");
//        toggleAR = testObject.AddComponent<ToggleAR>();

//        // 2. Création de la caméra de test
//        var cameraObject = new GameObject("TestCamera");
//        testCamera = cameraObject.AddComponent<Camera>();
//        testCamera.tag = "MainCamera";

//        // Configuration VR par défaut (Skybox)
//        testCamera.clearFlags = CameraClearFlags.Skybox;
//        testCamera.backgroundColor = Color.blue; // Une couleur VR par défaut

//        // Liaison manuelle
//        toggleAR.mainCamera = testCamera;
//    }

//    [TearDown]
//    public void TearDown()
//    {
//        // Nettoyage propre pour éviter les fuites mémoire dans l'éditeur
//        if (testObject != null) Object.DestroyImmediate(testObject);
//        if (testCamera != null) Object.DestroyImmediate(testCamera.gameObject);
//    }

//    // --- TESTS EXISTANTS (CORRIGÉS) ---

//    [Test]
//    public void MainCamera_WhenAssignedManually_IsNotNull()
//    {
//        Assert.IsNotNull(toggleAR.mainCamera);
//        Assert.AreEqual(testCamera, toggleAR.mainCamera);
//    }

//    [Test]
//    public void SwitchMode_WithNullARManager_DoesNotThrow()
//    {
//        // Cas : Pas de ARCameraManager sur la caméra
//        // On simule le Start pour initialiser les variables internes
//        toggleAR.SendMessage("Start");

//        Assert.DoesNotThrow(() => toggleAR.SwitchMode());
//    }

//    [Test]
//    public void SwitchMode_WithNullARManager_DoesNotChangeCameraFlags()
//    {
//        // Arrange
//        toggleAR.SendMessage("Start");
//        CameraClearFlags initialFlags = testCamera.clearFlags;

//        // Act
//        toggleAR.SwitchMode();

//        // Assert : Rien ne doit changer car le manager est absent
//        Assert.AreEqual(initialFlags, testCamera.clearFlags);
//    }

//    [Test]
//    public void SwitchMode_WithNullMainCamera_DoesNotThrow()
//    {
//        // Arrange : On détache la caméra pour forcer le null
//        toggleAR.mainCamera = null;

//        // Note : On ne lance pas Start() ici car Start() essaierait de retrouver la caméra via Tag

//        // Act & Assert
//        Assert.DoesNotThrow(() => toggleAR.SwitchMode());
//    }

//    [Test]
//    public void Start_WithNullMainCamera_FindsMainCameraByTag()
//    {
//        // Arrange
//        toggleAR.mainCamera = null;
//        testCamera.tag = "MainCamera"; // Important pour Camera.main

//        // Act
//        toggleAR.SendMessage("Start");

//        // Assert
//        Assert.IsNotNull(toggleAR.mainCamera, "Le script aurait dû trouver Camera.main automatiquement");
//        Assert.AreEqual(testCamera, toggleAR.mainCamera);
//    }

//    // --- NOUVEAU TEST CRUCIAL (HAPPY PATH) ---

//    [Test]
//    public void SwitchMode_WithARManager_TogglesToTransparentBlack()
//    {
//        // Arrange : On ajoute le composant requis pour que ça marche
//        testCamera.gameObject.AddComponent<ARCameraManager>();

//        // On lance l'initialisation
//        toggleAR.SendMessage("Start");

//        // Act : On active le mode AR
//        toggleAR.SwitchMode();

//        // Assert 1 : Le fond est-il devenu 'Solid Color' ?
//        Assert.AreEqual(CameraClearFlags.SolidColor, testCamera.clearFlags, "Le mode de rendu devrait être SolidColor en AR");

//        // Assert 2 : Le fond est-il bien Noir Transparent (0,0,0,0) ?
//        Assert.AreEqual(new Color(0, 0, 0, 0), testCamera.backgroundColor, "Le fond devrait être noir transparent pour le Passthrough");

//        // Act 2 : On re-clique pour revenir en VR
//        toggleAR.SwitchMode();

//        // Assert 3 : Est-on revenu en mode Skybox ?
//        Assert.AreEqual(CameraClearFlags.Skybox, testCamera.clearFlags, "Le mode de rendu devrait revenir à Skybox en VR");
//    }
//}
