#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace GSpawn
{
    public class ModularWallsObjectSpawn : ObjectSpawnTool
    {
        private enum CornerType
        {
            Inner = 0,
            Outer
        }

        private class WallPiece
        {
            public ObjectSpawnCellStack     stack;
            public ObjectSpawnCellSegment   segment;
            public ObjectSpawnCell          refCell;
            public WallPiece                previousPiece;
            public WallPiece                nextPiece;
            public ModularWallRuleId        ruleId;
            public GameObject               gameObject;     // Note: Needed when spawning pillars.

            public bool         closesWallLoop;
            public CornerType   closedCornerType;
            public WallPiece    closingPiece;

            public bool         isFirstCornerPart;
            public bool         isSecondCornerPart;

            public bool         skipped { get { return !isFirstCornerPart && isSecondCornerPart; } }
        }

        [NonSerialized]
        private ModularWallsObjectSpawnSettings     _settings;
        [NonSerialized]
        private ObjectModularSnapSettings           _modularSnapSettings;
        [SerializeField]
        private ObjectModularSnapSession            _modularSnapSession;

        [NonSerialized]
        private bool                                _isBuildingWalls        = false;
        [NonSerialized]
        private List<ObjectSpawnCellSegment>        _segments               = new List<ObjectSpawnCellSegment>();
        [NonSerialized]
        private List<WallPiece>                     _wallPieces             = new List<WallPiece>();
        [NonSerialized]
        private ObjectSpawnCellSegment              _penultimateSegment;
        [NonSerialized]
        private ObjectSpawnCellSegment              _lastSegment;
        [NonSerialized]
        private OBB                                 _refOBB;
        [NonSerialized]
        private Vector3                             _refInnerAxis;
        [NonSerialized]
        private ObjectSpawnExtensionPlane           _extensionPlane         = new ObjectSpawnExtensionPlane();

        private ModularWallPrefabProfile            wallPrefabProfile       { get { return settings.modularWallPrefabProfile; } }

        public ModularWallsObjectSpawnSettings      settings
        {
            get
            {
                if (_settings == null) _settings = AssetDbEx.loadScriptableObject<ModularWallsObjectSpawnSettings>(PluginFolders.settings);
                return _settings;
            }
        }
        public ObjectModularSnapSettings    modularSnapSettings
        {
            get
            {
                if (_modularSnapSettings == null) _modularSnapSettings = AssetDbEx.loadScriptableObject<ObjectModularSnapSettings>(PluginFolders.settings, typeof(ModularWallsObjectSpawn).Name + "_" + typeof(ObjectModularSnapSettings).Name);
                return _modularSnapSettings;
            }
        }
        public bool                         isBuildingWalls                 { get { return _isBuildingWalls; } }
        public override bool                requiresSpawnGuide              { get { return true; } }
        public override ObjectSpawnToolId   spawnToolId                     { get { return ObjectSpawnToolId.ModularWalls; } }
        public override bool                canChangeSpawnGuideTransform    { get { return !_isBuildingWalls; } }

        public override void setSpawnGuidePrefab(PluginPrefab prefab)
        {
            selectSpawnGuidePrefab();
        }

        public void onModularWallPrefabProfileChanged()
        {
            selectSpawnGuidePrefab();
        }

        public void stepBack()
        {
            if (!_isBuildingWalls || _segments.Count < 4) return;

            _segments.RemoveAt(_segments.Count - 1);
            _segments.RemoveAt(_segments.Count - 1);

            _lastSegment = _segments[_segments.Count - 1];
            _penultimateSegment = _segments[_segments.Count - 2];
        }

        public override void onNoLongerActive()
        {
            spawnGuide.destroyGuide();
            onCancelWallBuild();
        }

        public void executeModularSnapSessionCommand(ObjectModularSnapSessionCommand command)
        {
            _modularSnapSession.executeCommand(command);
        }

        private void selectSpawnGuidePrefab()
        {
            spawnGuide.destroyGuide();
            if (!validateWallProfile()) return;
            spawnGuide.usePrefab(wallPrefabProfile.getBestPrefabForSpawnGuide().pluginPrefab, _modularSnapSession);
        }

        private bool validateWallProfile()
        {
            if (!wallPrefabProfile.isAnyPrefabUsed(ModularWallRuleId.StraightWall))
            {
                Debug.LogError("No StraightWall prefabs were found in profile '" + wallPrefabProfile.profileName + "'. Make sure " +
                    "the StraightWall rule has at least one prefab assigned to it that is marked as 'Used'.");
                return false;
            }
            if (wallPrefabProfile.examplePrefab == null)
            {
                Debug.LogError("No example prefab found in profile '" + wallPrefabProfile.profileName + "'.");
                return false;
            }

            return true;
        }

        private void keepSpawnGuideAlignedToGridUp()
        {
            var gridUp      = PluginScene.instance.grid.up;
            Vector3 guideUp = wallPrefabProfile.getModularWallUpAxis(spawnGuide.gameObject);
            float dot       = Vector3.Dot(gridUp, guideUp);
            if (Mathf.Abs(dot - 1.0f) > 1e-5f)
            {
                spawnGuide.transform.alignAxis(guideUp, gridUp, spawnGuide.transform.position);
            }
        }

        protected override void doOnSceneGUI()
        {     
            Event e = Event.current;
            if (!_isBuildingWalls)
            {
                if (spawnGuide.isPresentInScene)
                {
                    if (!validateWallProfile())
                    {
                        spawnGuide.destroyGuide();
                        return;
                    }
                }

                spawnGuide.onSceneGUI();
                if (_modularSnapSession.isActive)
                {
                    keepSpawnGuideAlignedToGridUp();
                    if (e.isLeftMouseButtonDownEvent())
                    {
                        if (e.isLeftMouseButtonDownEvent())
                        {
                            if (e.noShiftCtrlCmdAlt()) onBeginWallBuild();
                        }
                    }
                }
            }
            else
            {
                if (FixedShortcuts.cancelAction(e) || spawnGuide.gameObject == null)
                {
                    onCancelWallBuild();
                    return;
                }
                else
                if (e.isRightMouseButtonDownEvent() && FixedShortcuts.selectionSegments_EnableStepBack(e))
                {
                    stepBack();
                    e.disable();
                }
                else
                if (e.isLeftMouseButtonDownEvent())
                {
                    if (FixedShortcuts.structureBuild_EnableCommitOnLeftClick(e))
                    {
                        onEndWallBuild();
                        return;
                    }
                    else appendSegmentsOnLeftMouseButtonDown();
                }

                if (e.isMouseMoveEvent()) updateSegments();
            }
        }

        protected override void onEnabled()
        {
            if (_modularSnapSession == null) _modularSnapSession = ScriptableObject.CreateInstance<ObjectModularSnapSession>();
            _modularSnapSession.sharedSettings = modularSnapSettings;

            Undo.undoRedoPerformed += onUndoRedo;
        }

        protected override void onDisabled()
        {
            Undo.undoRedoPerformed -= onUndoRedo;
        }

        protected override void onDestroy()
        {
            ScriptableObjectEx.destroyImmediate(_modularSnapSession);
        }

        protected override void draw()
        {
            if (!isSpawnGuidePresentInScene && !_isBuildingWalls) return;

            ObjectSpawnCellSegment.DrawConfig drawConfig = new ObjectSpawnCellSegment.DrawConfig();
            drawConfig.cellWireColor    = ObjectSpawnPrefs.instance.mdWallSpawnCellWireColor;
            drawConfig.drawGranular     = true;

            int numSegments = _segments.Count;
            for (int i = 0; i < numSegments; ++i)
            {
                var segment = _segments[i];
                segment.draw(drawConfig);
            }
        }

        private void onUndoRedo()
        {
            if (GSpawn.active.levelDesignToolId == LevelDesignToolId.ObjectSpawn &&
                ObjectSpawn.instance.activeToolId == ObjectSpawnToolId.ModularWalls)
            {
                selectSpawnGuidePrefab();
            }
        }

        private void onBeginWallBuild()
        {
            if (_isBuildingWalls) return;

            _segments.Clear();
            _refOBB         = calcRefOBB();
            _extensionPlane.set(_refOBB, Box3D.findMostAlignedFace(_refOBB.center, _refOBB.size, _refOBB.rotation, -PluginScene.instance.grid.up), 0.0f);

            var extensionAxis   = wallPrefabProfile.getModularWallForwardAxis(spawnGuide.gameObject);
            _penultimateSegment = new ObjectSpawnCellSegment(_refOBB, PluginScene.instance.grid.up, extensionAxis);
            _penultimateSegment.setObjectRotation(spawnGuide.transform.rotation);
            _penultimateSegment.setLength(1);

            _lastSegment = new ObjectSpawnCellSegment(_refOBB, _penultimateSegment.heightAxis, _penultimateSegment.rightAxis);
            _lastSegment.makeObjectRotation90DegreesRelativeToSegment(_penultimateSegment);

            _segments.Add(_penultimateSegment);
            _segments.Add(_lastSegment);

            spawnGuide.setGuideObjectActive(false);
            settings.ui.SetEnabled(false);       
            _isBuildingWalls = true;

            var ui = ModularWallPrefabProfileDbUI.instance;
            if (ui.isEnabledSelf) ui.setEnabled(false);
        }

        private void onEndWallBuild()
        {
            if (!_isBuildingWalls) return;

            spawnObjects();

            _segments.Clear();
            _isBuildingWalls = false;
            spawnGuide.setGuideObjectActive(true);
            settings.ui.SetEnabled(true);

            var ui = ModularWallPrefabProfileDbUI.instance;
            if (!ui.isEnabledSelf) ui.setEnabled(true);
        }

        private void onCancelWallBuild()
        {
            if (!_isBuildingWalls) return;

            _segments.Clear();
             _isBuildingWalls = false;
            spawnGuide.setGuideObjectActive(true);
            settings.ui.SetEnabled(true);

            var ui = ModularWallPrefabProfileDbUI.instance;
            if (!ui.isEnabledSelf) ui.setEnabled(true);
        }

        private void updateSegments()
        {
            Vector3 extPlaneIntersectPt;
            if (!_extensionPlane.cursorRaycast(out extPlaneIntersectPt)) return;

            Vector3 toIntersectPt = extPlaneIntersectPt - _penultimateSegment.startPosition;
            bool updatePenultimateLength = true;
            if (Vector3.Dot(toIntersectPt, _penultimateSegment.extensionAxis) < 0.0f)
            {
                if (_segments.Count == 2) _penultimateSegment.setExtensionAxis(-_penultimateSegment.extensionAxis);
                else updatePenultimateLength = false;
            }

            int oldLength;
            if (updatePenultimateLength)
            {
                oldLength = _penultimateSegment.snapLengthToCursor(_extensionPlane, settings.maxSegmentLength, false);
                _penultimateSegment.setHeight(1, oldLength);
            }

            toIntersectPt = extPlaneIntersectPt - _penultimateSegment.endPosition;
            if (Vector3.Dot(toIntersectPt, _lastSegment.extensionAxis) < 0.0f)
                _lastSegment.setExtensionAxis(-_lastSegment.extensionAxis);
            
            _lastSegment.makeObjectRotation90DegreesRelativeToSegment(_penultimateSegment);

            connectLastToPenultimate();

            oldLength = _lastSegment.snapLengthToCursor(_extensionPlane, settings.maxSegmentLength, true);
            _lastSegment.setHeight(1, oldLength);

            if (_segments.Count > 2)
            {
                detectOccludedCellsInSegment(_penultimateSegment, 0, _segments.Count - 3);
                detectOccludedCellsInSegment(_lastSegment, 0, _segments.Count - 2);
            }
        }

        private void appendSegmentsOnLeftMouseButtonDown()
        {
            var newPenultimateSegment   = new ObjectSpawnCellSegment(_lastSegment.refObjectOBB, _extensionPlane.planeNormal, _lastSegment.extensionAxis);
            newPenultimateSegment.setObjectRotation(_lastSegment.objectRotation);
            var newLastSegment          = new ObjectSpawnCellSegment(_lastSegment.refObjectOBB, _extensionPlane.planeNormal, _lastSegment.rightAxis);

            _lastSegment.removeLastStack();
            newPenultimateSegment.connectToParallelSegmentEnd(_lastSegment, 0.0f);

            int oldLength = newPenultimateSegment.snapLengthToCursor(_extensionPlane, settings.maxSegmentLength, false);
            newPenultimateSegment.setHeight(1, oldLength);
            newLastSegment.makeObjectRotation90DegreesRelativeToSegment(newPenultimateSegment);

            _penultimateSegment = newPenultimateSegment;
            _lastSegment = newLastSegment;

            _segments.Add(newPenultimateSegment);
            _segments.Add(newLastSegment);

            connectLastToPenultimate();
        }

        private void connectLastToPenultimate()
        {
            ObjectSpawnCell connectionCell = _penultimateSegment.lastStack.getCell(0);
            var cornerType = getCornerType(_penultimateSegment, _lastSegment);
            if (cornerType == CornerType.Outer)
            {
                _lastSegment.setStartPosition(connectionCell.objectOBBCenter +
                    _penultimateSegment.extensionAxis * wallPrefabProfile.straight_ForwardPushDistance_Outer +
                    _lastSegment.extensionAxis * wallPrefabProfile.straight_InnerPushDistance_Outer);
            }
            else
            {
                _lastSegment.setStartPosition(connectionCell.objectOBBCenter +
                    _penultimateSegment.extensionAxis * wallPrefabProfile.straight_ForwardPushDistance_Inner +
                    _lastSegment.extensionAxis * wallPrefabProfile.straight_InnerPushDistance_Inner);
            }
        }

        private CornerType getCornerType(ObjectSpawnCellSegment firstSegment, ObjectSpawnCellSegment secondSegment)
        {
            Vector3 firstInnerAxis = firstSegment.objectRotation * _refInnerAxis;
            return Vector3.Dot(firstInnerAxis, secondSegment.extensionAxis) > 0.0f ? CornerType.Inner : CornerType.Outer;
        }

        private void detectOccludedCellsInSegment(ObjectSpawnCellSegment segment, int firstSegmentIndex, int lastSegmentIndex)
        {
            // Reset occlusion state for all cells in the segment
            segment.setAllCellsOccluded(false);

            // Store segment data for easy access. This is the segment that will
            // have its cells occluded when they intersect cells in other segments.
            OBB segmentOBB = segment.obb;
            if (!segmentOBB.isValid) return;
            int numStacks = segment.numStacks;
            if (numStacks == 0) return;

            // Store the cell size along the segment's extension axis here. We will
            // need it when we perform the cell occlusion test.
            float cellSizeAlongExtAxis = segment.calcCellSizeAlongAxis(segment.extensionAxis);

            // Loop through each segment in the specified range and check for occlusions
            for (int otherSegmentIndex = firstSegmentIndex; otherSegmentIndex <= lastSegmentIndex; ++otherSegmentIndex)
            {
                // Store the other segment's OBB for easy access
                var otherSegment = _segments[otherSegmentIndex];
                OBB otherSegmentOBB = otherSegment.obb;

                // If the other segment's OBB is invalid or if the segments' OBBs don't intersect, move on to the next segment.
                if (!otherSegmentOBB.isValid) continue;
                if (!segmentOBB.intersectsOBB(otherSegmentOBB)) continue;       // Note: Broad-Phase Level 1

                // Loop through each stack in the segment
                int numOtherStacks = otherSegment.numStacks;
                for (int stackIndex = 0; stackIndex < numStacks; ++stackIndex)
                {
                    // Store stack data for easy access and move on if the stack OBB is invalid
                    var stack = segment.getStack(stackIndex);
                    OBB stackOBB = stack.obb;
                    if (!stackOBB.isValid) continue;

                    // If this stack OBB doesn't intersect the segment's OBB, move on.
                    if (!stackOBB.intersectsOBB(otherSegmentOBB)) continue;     // Note: Broad-Phase Level 2

                    // Now loop through each stack in the segment we are testing against
                    for (int otherStackIndex = 0; otherStackIndex < numOtherStacks; ++otherStackIndex)
                    {
                        // Store data easy access and move on if the stack's OBB is invalid
                        var otherStack = otherSegment.getStack(otherStackIndex);
                        OBB otherStackOBB = otherStack.obb;
                        if (!otherStackOBB.isValid) continue;

                        // Note: Broad-Phase Level 3
                        //       We only proceed if the stacks occlude each other.
                        float d = (stack.startPosition - otherStack.startPosition).magnitude;
                        if (d < 1e-5f)
                        {
                            // Stack positions are close enough. Check size. If the sizes don't match, we can move on.
                            float otherCellSize = otherSegment.calcCellSizeAlongAxis(segment.extensionAxis);
                            if (Mathf.Abs(cellSizeAlongExtAxis - otherCellSize) < 1e-5f) stack.setAllCellsOccluded(true);
                        }
                    }
                }
            }
        }

        [NonSerialized]
        List<GameObject>        _overlappedWallPieces   = new List<GameObject>();
        [NonSerialized]
        ObjectOverlapFilter     _wallOverlapFilter      = new ObjectOverlapFilter();
        [NonSerialized]
        HashSet<GameObject>     _spawnedWallPieces      = new HashSet<GameObject>();
        private void spawnObjects()
        {
            createWallPieces();
            processWallPieces();

            const float errorEps        = 1e-5f;
            Vector3     heightOffset    = _extensionPlane.planeNormal * wallPrefabProfile.wallHeight;
            int         numWallPieces   = _wallPieces.Count;

            Vector3     objectPosition;
            Quaternion  objectRotation;
            Vector3     avgCenter;
            Vector3     firstInnerAxis;
            Vector3     secondInnerAxis;
            Quaternion  avgRotation;

            _spawnedWallPieces.Clear();
            ObjectBounds.QueryConfig wallBoundsQConfig  = wallPrefabProfile.getWallBoundsQConfig();
            ObjectOverlapConfig wallOverlapConfig       = ObjectOverlapConfig.defaultConfig;
            wallOverlapConfig.prefabMode                = ObjectOverlapPrefabMode.OnlyPrefabInstanceRoot;

            _wallOverlapFilter.objectTypes              = GameObjectType.Mesh;
            _wallOverlapFilter.customFilter             = (GameObject go) => { return (go.isObjectModularWallPiece() || go.isObjectModularWallPillar()) && !_spawnedWallPieces.Contains(go); };

            for (int pieceIndex = 0; pieceIndex < numWallPieces; ++pieceIndex)
            {
                var wallPiece       = _wallPieces[pieceIndex];
                if (wallPiece.skipped) continue;

                ObjectSpawnCell refCell = wallPiece.refCell;
                switch (wallPiece.ruleId)
                {
                    case ModularWallRuleId.StraightWall:

                        objectPosition = refCell.objectOBBCenter + refCell.objectOBBRotation * wallPrefabProfile.straightRT.position;
                        objectRotation = refCell.objectOBBRotation * wallPrefabProfile.straightRT.rotation;
                        break;

                    case ModularWallRuleId.InnerCorner:

                        firstInnerAxis  = wallPiece.segment.objectRotation * _refInnerAxis;
                        secondInnerAxis = wallPiece.nextPiece.segment.objectRotation * _refInnerAxis;

                        avgCenter       = (refCell.objectOBBCenter + wallPiece.nextPiece.refCell.objectOBBCenter) / 2;
                        avgRotation     = Quaternion.LookRotation((firstInnerAxis + secondInnerAxis).normalized, wallPiece.segment.heightAxis);

                        objectPosition  = avgCenter + avgRotation * wallPrefabProfile.innerCornerRT.position;
                        objectRotation  = avgRotation * wallPrefabProfile.innerCornerRT.rotation;
                        break;

                    case ModularWallRuleId.OuterCorner:

                        firstInnerAxis  = wallPiece.segment.objectRotation * _refInnerAxis;
                        secondInnerAxis = wallPiece.nextPiece.segment.objectRotation * _refInnerAxis;

                        avgCenter       = (refCell.objectOBBCenter + wallPiece.nextPiece.refCell.objectOBBCenter) / 2;
                        avgRotation     = Quaternion.LookRotation((firstInnerAxis + secondInnerAxis).normalized, wallPiece.segment.heightAxis);

                        objectPosition  = avgCenter + avgRotation * wallPrefabProfile.outerCornerRT.position;
                        objectRotation  = avgRotation * wallPrefabProfile.outerCornerRT.rotation;
                        break;

                    default:

                        continue;
                }

                int numCells = wallPiece.stack.numCells;
                for (int cellIndex = 0; cellIndex < numCells; ++cellIndex)
                {
                    Vector3 pos                 = (objectPosition + heightOffset * cellIndex).roundCorrectError(errorEps);
                    PluginPrefab pluginPrefab   = wallPrefabProfile.pickPrefab(wallPiece.ruleId).pluginPrefab;
                    wallPiece.gameObject        = pluginPrefab.spawn(pos, objectRotation, Vector3.one);
                    _spawnedWallPieces.Add(wallPiece.gameObject);

                    if (settings.eraseExisting)
                    {
                        OBB obb = ObjectBounds.calcHierarchyWorldOBB(wallPiece.gameObject, wallBoundsQConfig);
                        if (obb.isValid)
                        {
                            obb.inflate(-0.01f);
                            if (PluginScene.instance.overlapBox(obb, _wallOverlapFilter, wallOverlapConfig, _overlappedWallPieces))
                                UndoEx.destroyGameObjectsImmediate(_overlappedWallPieces);
                        }
                    }

                    if (wallPiece.ruleId != ModularWallRuleId.StraightWall)
                        wallPiece.nextPiece.gameObject = wallPiece.gameObject;
                }
            }

            _spawnedWallPieces.Clear();
            spawnPillars();
        }

        private void createWallPieces()
        {
            _wallPieces.Clear();

            int numSegments     = _segments.Count;
            for (int segIndex = 0; segIndex < numSegments; ++segIndex)
            {
                var segment     = _segments[segIndex];
                int numStacks   = segment.numStacks;
                for (int stackIndex = 0; stackIndex < numStacks; ++stackIndex)
                {
                    var stack           = segment.getStack(stackIndex);
                    if (!stack.getCell(0).isGoodForSpawn) continue;

                    var wallPiece       = new WallPiece();
                    wallPiece.stack     = stack;
                    wallPiece.segment   = segment;
                    _wallPieces.Add(wallPiece);

                    if (_wallPieces.Count > 1) 
                    {
                        _wallPieces[_wallPieces.Count - 2].nextPiece = wallPiece;
                        wallPiece.previousPiece = _wallPieces[_wallPieces.Count - 2];
                    }
                }
            }
        }

        private void processWallPieces()
        {
            bool hasInnerCorner = wallPrefabProfile.isAnyPrefabUsed(ModularWallRuleId.InnerCorner);
            bool hasOuterCorner = wallPrefabProfile.isAnyPrefabUsed(ModularWallRuleId.OuterCorner);

            int numPieces = _wallPieces.Count;
            for (int i = 0; i < numPieces; ++i)
            {
                var piece       = _wallPieces[i];
                var nextPiece   = piece.nextPiece;

                if (piece.isSecondCornerPart) continue;
                else
                {
                    if (nextPiece != null)
                    {
                        float absDot = Vector3Ex.absDot(piece.segment.extensionAxis, nextPiece.segment.extensionAxis);
                        if (absDot < 1e-5f)
                        {
                            var cornerType          = getCornerType(piece.segment, nextPiece.segment);
                            if (cornerType == CornerType.Outer && hasOuterCorner)
                            {
                                piece.ruleId        = ModularWallRuleId.OuterCorner;
                                piece.refCell       = piece.stack.getCell(0);
                                nextPiece.ruleId    = ModularWallRuleId.OuterCorner;
                                nextPiece.refCell   = nextPiece.stack.getCell(0);

                                piece.isFirstCornerPart         = true;
                                nextPiece.isSecondCornerPart    = true;
                            }
                            else
                            if (cornerType == CornerType.Inner && hasInnerCorner)
                            {
                                piece.ruleId        = ModularWallRuleId.InnerCorner;
                                piece.refCell       = piece.stack.getCell(0);
                                nextPiece.ruleId    = ModularWallRuleId.InnerCorner;
                                nextPiece.refCell   = nextPiece.stack.getCell(0);

                                piece.isFirstCornerPart         = true;
                                nextPiece.isSecondCornerPart    = true;
                            }
                            else
                            {
                                piece.ruleId    = ModularWallRuleId.StraightWall;
                                piece.refCell   = piece.stack.getCell(0);
                            }
                        }
                        else
                        {
                            piece.ruleId    = ModularWallRuleId.StraightWall;
                            piece.refCell   = piece.stack.getCell(0);
                        }
                    }
                    else
                    {
                        piece.ruleId    = ModularWallRuleId.StraightWall;
                        piece.refCell   = piece.stack.getCell(0);
                    }
                }
            }

            // Handle special case where the last piece forms a closed loop with the first piece
            if (numPieces > 1)
            {
                WallPiece firstPiece    = _wallPieces[0];
                WallPiece lastPiece     = _wallPieces[numPieces - 1];
                float absDot            = Vector3Ex.absDot(firstPiece.segment.extensionAxis, lastPiece.segment.extensionAxis);
                if (absDot > 1e-5f) return;

                OBB firstOBB            = firstPiece.refCell.objectOBB;
                OBB secondOBB           = lastPiece.refCell.objectOBB;
                Vector3 vec             = secondOBB.center - firstOBB.center;
                float d0                = Vector3Ex.absDot(vec, firstPiece.segment.extensionAxis);
                float d1                = Vector3Ex.absDot(vec, lastPiece.segment.extensionAxis);
                                  
                CornerType cornerType = getCornerType(lastPiece.segment, firstPiece.segment);
                if (cornerType == CornerType.Inner && hasInnerCorner)
                {
                    if (Mathf.Abs(d0 - wallPrefabProfile.straight_ForwardPushDistance_Inner) < 1e-4f &&
                        Mathf.Abs(d1 - wallPrefabProfile.straight_ForwardPushDistance_Inner) < 1e-4f)
                    {
                        if (!firstPiece.isFirstCornerPart && !lastPiece.isSecondCornerPart)
                        {
                            firstPiece.isSecondCornerPart   = true;
                            firstPiece.ruleId               = ModularWallRuleId.InnerCorner;

                            lastPiece.closesWallLoop        = true;
                            lastPiece.closedCornerType      = cornerType;
                            lastPiece.closingPiece          = firstPiece;
                            lastPiece.nextPiece             = firstPiece;
                            lastPiece.ruleId                = ModularWallRuleId.InnerCorner;
                        }
                    }
                }
                else
                if (cornerType == CornerType.Outer && hasOuterCorner) 
                {
                    if (Mathf.Abs(d0 - wallPrefabProfile.straight_ForwardPushDistance_Outer) < 1e-4f &&
                        Mathf.Abs(d1 - wallPrefabProfile.straight_ForwardPushDistance_Outer) < 1e-4f)
                    {
                        if (!firstPiece.isFirstCornerPart && !lastPiece.isSecondCornerPart) 
                        {
                            firstPiece.isSecondCornerPart   = true;
                            firstPiece.ruleId               = ModularWallRuleId.OuterCorner;

                            lastPiece.closesWallLoop        = true;
                            lastPiece.closedCornerType      = cornerType;
                            lastPiece.closingPiece          = firstPiece;
                            lastPiece.nextPiece             = firstPiece;
                            lastPiece.ruleId                = ModularWallRuleId.OuterCorner;
                        }
                    }
                }
            }
        }

        [NonSerialized]
        private List<GameObject> _spawnedPillars = new List<GameObject>();
        private void spawnPillars()
        {
            _spawnedPillars.Clear();
            if (!wallPrefabProfile.spawnPillars || !wallPrefabProfile.hasPillars) return;

            var pillarPrefabProfile = wallPrefabProfile.pillarProfile;
            int numWallPieces       = _wallPieces.Count;
            for (int i = 0; i < numWallPieces; ++i)
            {
                var piece = _wallPieces[i];

                Transform wallTransform = piece.gameObject.transform;
                // Note: Yes, hasInnerCornerPillarBegin.
                if (piece.ruleId == ModularWallRuleId.StraightWall && wallPrefabProfile.hasInnerCornerPillarBegin)
                {
                    // Spawn 2 pillars for both caps
                    Vector3 objectPosition      = wallTransform.position + wallTransform.rotation * wallPrefabProfile.pillarMidStraightBeginRT.position;
                    Quaternion objectRotation   = wallTransform.rotation * wallPrefabProfile.pillarMidStraightBeginRT.rotation;

                    PluginPrefab pluginPrefab   = pillarPrefabProfile.pickPrefab().pluginPrefab;
                    _spawnedPillars.Add(pluginPrefab.spawn(objectPosition, objectRotation, Vector3.one));

                    objectPosition              = wallTransform.position + wallTransform.rotation * wallPrefabProfile.pillarMidStraightEndRT.position;
                    objectRotation              = wallTransform.rotation * wallPrefabProfile.pillarMidStraightEndRT.rotation;

                    pluginPrefab                = pillarPrefabProfile.pickPrefab().pluginPrefab;
                    _spawnedPillars.Add(pluginPrefab.spawn(objectPosition, objectRotation, Vector3.one));
                }
                else
                if (piece.ruleId == ModularWallRuleId.InnerCorner && wallPrefabProfile.hasInnerCornerPillarBegin && piece.isFirstCornerPart)
                {
                    Vector3 objectPosition      = wallTransform.position + wallTransform.rotation * wallPrefabProfile.pillarInnerCornerBeginRT.position;
                    Quaternion objectRotation   = wallTransform.rotation * wallPrefabProfile.pillarInnerCornerBeginRT.rotation;

                    PluginPrefab pluginPrefab   = pillarPrefabProfile.pickPrefab().pluginPrefab;
                    _spawnedPillars.Add(pluginPrefab.spawn(objectPosition, objectRotation, Vector3.one));
                }
                else
                if (piece.ruleId == ModularWallRuleId.InnerCorner && wallPrefabProfile.hasInnerCornerPillarEnd && piece.isSecondCornerPart)
                {
                    Vector3 objectPosition      = wallTransform.position + wallTransform.rotation * wallPrefabProfile.pillarInnerCornerEndRT.position;
                    Quaternion objectRotation   = wallTransform.rotation * wallPrefabProfile.pillarInnerCornerEndRT.rotation;

                    PluginPrefab pluginPrefab   = pillarPrefabProfile.pickPrefab().pluginPrefab;
                    _spawnedPillars.Add(pluginPrefab.spawn(objectPosition, objectRotation, Vector3.one));
                }
                else
                if (piece.ruleId == ModularWallRuleId.OuterCorner && wallPrefabProfile.hasOuterCornerPillarBegin && piece.isFirstCornerPart)
                {
                    Vector3 objectPosition      = wallTransform.position + wallTransform.rotation * wallPrefabProfile.pillarOuterCornerBeginRT.position;
                    Quaternion objectRotation   = wallTransform.rotation * wallPrefabProfile.pillarOuterCornerBeginRT.rotation;

                    PluginPrefab pluginPrefab = pillarPrefabProfile.pickPrefab().pluginPrefab;
                    _spawnedPillars.Add(pluginPrefab.spawn(objectPosition, objectRotation, Vector3.one));
                }
                else
                if (piece.ruleId == ModularWallRuleId.OuterCorner && wallPrefabProfile.hasOuterCornerPillarEnd && piece.isSecondCornerPart)
                {
                    Vector3 objectPosition      = wallTransform.position + wallTransform.rotation * wallPrefabProfile.pillarOuterCornerEndRT.position;
                    Quaternion objectRotation   = wallTransform.rotation * wallPrefabProfile.pillarOuterCornerEndRT.rotation;

                    PluginPrefab pluginPrefab   = pillarPrefabProfile.pickPrefab().pluginPrefab;
                    _spawnedPillars.Add(pluginPrefab.spawn(objectPosition, objectRotation, Vector3.one));
                }
            }

            fixPillarOverlaps(_spawnedPillars);
            _spawnedPillars.Clear();
        }

        [NonSerialized] ObjectOverlapFilter _pillarOverlapFilter    = new ObjectOverlapFilter();
        [NonSerialized] List<GameObject>    _overlappedPillars      = new List<GameObject>();
        private void fixPillarOverlaps(List<GameObject> pillars)
        {
            ObjectBounds.QueryConfig boundsQConfig  = ObjectBounds.QueryConfig.defaultConfig;
            boundsQConfig.objectTypes               = GameObjectType.Mesh | GameObjectType.Sprite;

            const float posEps = 1e-3f;
            int numPillars = pillars.Count;
            for (int i = 0; i < numPillars; ++i)
            {
                var pillar = pillars[i];

                // Note: Could have been destroyed inside the inner loop.
                if (pillar == null) continue;

                OBB pillarOBB = ObjectBounds.calcWorldOBB(pillar, boundsQConfig);
                if (!pillarOBB.isValid) continue;

                for (int j = i + 1; j < numPillars; ++j)
                {
                    var otherPillar = pillars[j];
                    if (otherPillar == null) continue;

                    var otherPillarOBB = ObjectBounds.calcWorldOBB(otherPillar, boundsQConfig);
                    if (!otherPillarOBB.isValid) continue;

                    if (Vector3.Magnitude(otherPillarOBB.center - pillarOBB.center) < posEps)
                    {
                        ObjectEvents.onObjectWillBeDestroyed(otherPillar);
                        GameObject.DestroyImmediate(otherPillar);
                    }
                }
            }

            // Handle overlaps with existing pillars
            if (settings.eraseExisting)
            {
                pillars.RemoveAll(item => item == null);

                ObjectOverlapConfig pillarOverlapConfig = ObjectOverlapConfig.defaultConfig;
                pillarOverlapConfig.prefabMode          = ObjectOverlapPrefabMode.OnlyPrefabInstanceRoot;
                _pillarOverlapFilter.objectTypes        = GameObjectType.Mesh | GameObjectType.Sprite;
                _pillarOverlapFilter.setIgnoredObjects(pillars);
                _pillarOverlapFilter.customFilter       = (GameObject go) => { return go.isObjectModularWallPillar(); };

                numPillars = pillars.Count;
                for (int i = 0; i < numPillars; ++i)
                {
                    var pillar      = pillars[i];
                    OBB pillarOBB   = ObjectBounds.calcWorldOBB(pillar, boundsQConfig);

                    if (PluginScene.instance.overlapBox(pillarOBB, _pillarOverlapFilter, pillarOverlapConfig, _overlappedPillars))
                    {
                        int numOverlapped = _overlappedPillars.Count;
                        for (int j = 0; j < numOverlapped; ++j)
                        {
                            GameObject overlapped   = _overlappedPillars[j];
                            OBB overlappedOBB       = ObjectBounds.calcWorldOBB(overlapped, boundsQConfig);

                            if (Vector3.Magnitude(overlappedOBB.center - pillarOBB.center) < posEps)
                            {
                                ObjectEvents.onObjectWillBeDestroyed(overlapped);
                                UndoEx.destroyGameObjectImmediate(overlapped);
                            }
                        }
                    }
                }
            }
        }

        private OBB calcRefOBB()
        {
            OBB obb = calcSpawnGuideWorldOBB();

            Vector3 obbSize         = obb.size;
            var axisDesc            = wallPrefabProfile.getModularWallUpAxisDesc(spawnGuide.gameObject);
            obbSize[axisDesc.index] = wallPrefabProfile.wallHeight;
            axisDesc                = wallPrefabProfile.getModularWallForwardAxisDesc(spawnGuide.gameObject);
            obbSize[axisDesc.index] = wallPrefabProfile.wallForwardSize;
            obb.size                = obbSize;

            Quaternion oldRotation  = spawnGuide.transform.rotation;
            spawnGuide.transform.rotation = Quaternion.identity;
            _refInnerAxis           = wallPrefabProfile.getModularWallInnerAxis(spawnGuide.gameObject);
            spawnGuide.transform.rotation = oldRotation;

            return obb;
        }
    }
}
#endif