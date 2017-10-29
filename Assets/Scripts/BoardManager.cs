using UnityEngine;
using System.Collections;

namespace TBT.GemsAndCombos {

    public class BoardManager : MonoBehaviour {

        private struct Gem {
            public int color;
            public GameObject gemObject;
            public GemHandler GM;
            public int dropDistance;
            public int matchNumber;
        }
        private Gem[,] board = new Gem[6, 5];
        private Gem gemClone = new Gem();

        [SerializeField] private Material mFire;
        [SerializeField] private Material mWater;
        [SerializeField] private Material mWood;
        [SerializeField] private Material mLight;
        [SerializeField] private Material mDark;
        [SerializeField] private Material mHeart;

        private Vector2 mousePos = Vector2.zero;
        private int heldGemX = 0, heldGemY = 0;
        private bool boardLocked = false;
        private bool mouseDown = false;



        private void Start () {
            InitBoard();
        }

        private void Update () {
            if (boardLocked) return;

            if (Input.GetMouseButtonDown(0)) {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseDown = true;
                CheckGemClick();
            } else if (Input.GetMouseButton(0) && mouseDown) {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CheckGemSwap();
            }

            if (Input.GetMouseButtonUp(0) && mouseDown) {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                DropGem();
                boardLocked = true;
                StartCoroutine(MatchGems());
                mouseDown = false;
            }
        }



        private void InitBoard () {
            for (int y = 0; y < 5; y++) {
                for (int x = 0; x < 6; x++) {
                    SpawnGem(x, y, 5);
                }
            }

            SkyfallGems();
        }

        private void SpawnGem (int x, int y, int drop) {
            GameObject gem = (GameObject)Instantiate(Resources.Load("Gem"), new Vector2(x, y + 5), Quaternion.identity);

            if (board[x, y].gemObject != null)
                DestroyObject(board[x, y].gemObject);

            board[x, y] = new Gem {
                gemObject = gem,
                dropDistance = drop,
                GM = gem.GetComponent<GemHandler>(),
                matchNumber = 0
            };

            SetColor(x, y);
        }

        private void SetColor (int x, int y) {
            int randomColor = Random.Range(0, 6);

            switch (randomColor) {
                case 0: // fire
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mFire;
                    break;
                case 1: // water
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mWater;
                    break;
                case 2: // wood
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mWood;
                    break;
                case 3: // light
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mLight;
                    break;
                case 4: // dark
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mDark;
                    break;
                case 5: // heart
                    board[x, y].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = mHeart;
                    break;
            }

            board[x, y].color = randomColor;
        }

        private void SkyfallGems () {
            for (int y = 0; y < 5; y++) {
                for (int x = 0; x < 6; x++) {
                    if (board[x, y].dropDistance > 0) {
                        board[x, y].GM.InitDrop(board[x, y].dropDistance);
                        board[x, y].dropDistance = 0;
                    }
                }
            }
        }

        private void CheckGemClick () {
            int clickX = Mathf.RoundToInt(mousePos.x);
            int clickY = Mathf.RoundToInt(mousePos.y);

            if (clickX > 5) clickX = 5;
            if (clickY > 4) clickY = 4;
            if (clickX < 0) clickX = 0;
            if (clickY < 0) clickY = 0;

            board[clickX, clickY].GM.mouseGem = true;
            heldGemX = clickX;
            heldGemY = clickY;

            CreateNewGemClone(clickX, clickY);

            Material gemMat = board[clickX, clickY].gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material;
            Color32 gemCol = gemMat.color;
            gemCol.a = 60;

            gemClone.gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = gemMat;
            gemClone.gemObject.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = gemCol;
        }

        private void CheckGemSwap () {
            int mouseX = Mathf.RoundToInt(mousePos.x);
            int mouseY = Mathf.RoundToInt(mousePos.y);

            if (mouseX > 5) mouseX = 5;
            if (mouseY > 4) mouseY = 4;
            if (mouseX < 0) mouseX = 0;
            if (mouseY < 0) mouseY = 0;

            if (mouseX != heldGemX || mouseY != heldGemY) {
                if (mouseX - heldGemX > 1)
                    mouseX = heldGemX + 1;
                if (heldGemX - mouseX > 1)
                    mouseX = heldGemX - 1;
                if (mouseY - heldGemY > 1)
                    mouseY = heldGemY + 1;
                if (heldGemY - mouseY > 1)
                    mouseY = heldGemY - 1;

                StartCoroutine(SwapGems(mouseX, mouseY));
            }
        }

        private IEnumerator SwapGems (int newGemX, int newGemY) {
            Vector3 targetAngle;
            GameObject gemSwapper = new GameObject();
            WaitForSeconds swapLoopTimer = new WaitForSeconds(0.01f);
            float swapLerpPercent = 0f;
            int oldGemX = heldGemX, oldGemY = heldGemY;

            heldGemX = newGemX; heldGemY = newGemY;

            Gem tempGem = board[newGemX, newGemY];
            board[newGemX, newGemY] = board[oldGemX, oldGemY];
            board[oldGemX, oldGemY] = tempGem;

            if (gemClone.gemObject.transform.parent != null) {
                gemClone.gemObject.transform.parent = null;
                gemClone.gemObject.transform.position = new Vector2(oldGemX, oldGemY);
            }

            if (board[oldGemX, oldGemY].gemObject.transform.parent != null) {
                board[oldGemX, oldGemY].gemObject.transform.parent = null;
                board[oldGemX, oldGemY].gemObject.transform.position = new Vector2(newGemX, newGemY);
            }

            targetAngle = new Vector3(0, 0, 180f);
            gemSwapper.transform.position = new Vector2(oldGemX - ((oldGemX - newGemX) / 2f), oldGemY - ((oldGemY - newGemY) / 2f));

            gemClone.gemObject.transform.parent = board[oldGemX, oldGemY].gemObject.transform.parent = gemSwapper.transform;

            while (swapLerpPercent <= 1f) {
                gemSwapper.transform.eulerAngles = Vector3.Lerp(Vector3.zero, targetAngle, swapLerpPercent);
                swapLerpPercent += 0.1f;
                yield return swapLoopTimer;
            }

            gemSwapper.transform.eulerAngles = targetAngle;

            if (board[oldGemX, oldGemY].gemObject.transform.parent == gemSwapper.transform)
                board[oldGemX, oldGemY].gemObject.transform.parent = null;
            if (gemClone.gemObject != null)
                if (gemClone.gemObject.transform.parent == gemSwapper.transform)
                    gemClone.gemObject.transform.parent = null;

            DestroyObject(gemSwapper);
        }

        private void DropGem () {
            board[heldGemX, heldGemY].GM.mouseGem = false;
            board[heldGemX, heldGemY].gemObject.transform.position = new Vector2(heldGemX, heldGemY);
            if (gemClone.gemObject != null)
                DestroyObject(gemClone.gemObject);
        }

        private void CreateNewGemClone (int cloneX, int cloneY) {
            GameObject gem = (GameObject)Instantiate(Resources.Load("Gem"), new Vector2(cloneX, cloneY), Quaternion.identity);
            gemClone = new Gem {
                color = board[cloneX, cloneY].color,
                gemObject = gem,
                GM = gem.GetComponent<GemHandler>()
            };
    }

        private IEnumerator MatchGems () {
            bool matchMade = false;
            int matchCount = 0, oldMatchCount = 0;

            // Slight beginning delay
            yield return new WaitForSeconds(0.5f);
            
            for (int y = 0; y < 5; y++) {
                for (int x = 0; x < 6; x++) {
                    int currentColor = board[x, y].color;
                    int oldMatchFound = 0;

                    // Checking for matches to the right
                    if (x < 4) {
                        int z = 1;
                        
                        while (x + z < 6) {
                            if (board[x + z, y].color == currentColor) {
                                if (board[x + z, y].matchNumber > 0 && (oldMatchFound > board[x + z, y].matchNumber || oldMatchFound == 0))
                                    oldMatchFound = board[x + z, y].matchNumber;
                                z++;
                            } else break;
                        }

                        if (z > 2) {
                            matchMade = true;
                            if (matchCount == oldMatchCount) matchCount++;
                            for (int i = 0; i < z; i++) {
                                if (oldMatchFound > 0)
                                    board[x + i, y].matchNumber = oldMatchFound;
                                else board[x + i, y].matchNumber = matchCount;
                            }
                        }
                    }

                    // Checking for matches to the left
                    if (x > 1) {
                        int z = 1;

                        while (x - z > -1) {
                            if (board[x - z, y].color == currentColor) {
                                if (board[x - z, y].matchNumber > 0 && (oldMatchFound > board[x - z, y].matchNumber || oldMatchFound == 0))
                                    oldMatchFound = board[x - z, y].matchNumber;
                                z++;
                            } else break;
                        }

                        if (z > 2) {
                            matchMade = true;
                            if (matchCount == oldMatchCount) matchCount++;
                            for (int i = 0; i < z; i++) {
                                if (oldMatchFound > 0)
                                    board[x - i, y].matchNumber = oldMatchFound;
                                else board[x - i, y].matchNumber = matchCount;
                            }
                        }
                    }

                    // Checking for matches up above
                    if (y < 3) {
                        int z = 1;

                        while (y + z < 5) {
                            if (board[x, y + z].color == currentColor) {
                                if (board[x, y + z].matchNumber > 0 && (oldMatchFound > board[x, y + z].matchNumber || oldMatchFound == 0))
                                    oldMatchFound = board[x, y + z].matchNumber;
                                z++;
                            } else break;
                        }

                        if (z > 2) {
                            matchMade = true;
                            if (matchCount == oldMatchCount) matchCount++;
                            for (int i = 0; i < z; i++) {
                                if (oldMatchFound > 0)
                                    board[x, y + i].matchNumber = oldMatchFound;
                                else board[x, y + i].matchNumber = matchCount;
                            }
                        }
                    }

                    // Checking for matches down below
                    if (y > 1) {
                        int z = 1;

                        while (y - z > -1) {
                            if (board[x, y - z].color == currentColor) {
                                if (board[x, y - z].matchNumber > 0 && (oldMatchFound > board[x, y - z].matchNumber || oldMatchFound == 0))
                                    oldMatchFound = board[x, y - z].matchNumber;
                                z++;
                            } else break;
                        }

                        if (z > 2) {
                            matchMade = true;
                            if (matchCount == oldMatchCount) matchCount++;
                            for (int i = 0; i < z; i++) {
                                if (oldMatchFound > 0)
                                    board[x, y - i].matchNumber = oldMatchFound;
                                else board[x, y - i].matchNumber = matchCount;
                            }
                        }
                    }

                    oldMatchCount = matchCount;
                }
            }

            // Remove all currently matched gems, I'm going to do it a janky way
            if (matchMade) {
                for (int zz = 1; zz <= matchCount; zz++) {
                    for (int yy = 0; yy < 5; yy++) {
                        for (int xx = 0; xx < 6; xx++) {
                            if (board[xx, yy].matchNumber == zz) 
                                DestroyObject(board[xx, yy].gemObject);
                        }
                    }

                    yield return new WaitForSeconds(0.2f);
                }
            
                DropRemainingGems();
                DropNewGems();
            } else {
                yield return new WaitForSeconds(0.25f);
                boardLocked = false;
            }
        }

        private void DropRemainingGems () {
            for (int y = 1; y < 5; y++) {
                for (int x = 0; x < 6; x++) {
                    if (board[x, y].matchNumber > 0) continue;

                    int dropGem = 0;
                    for (int i = 1; i <= y; i++) {
                        if (board[x, y - i].matchNumber > 0)
                            dropGem++;
                    }

                    if (dropGem > 0) {
                        board[x, y].dropDistance = dropGem;

                        Gem tempGem = board[x, y - dropGem];
                        board[x, y - dropGem] = board[x, y];
                        board[x, y] = tempGem;
                    }
                }
            }
        }

        private void DropNewGems () {
            for (int y = 4; y >= 0; y--) {
                for (int x = 0; x < 6; x++) {
                    if (board[x, y].matchNumber > 0) {
                        SpawnGem(x, y, 5);
                    }
                }
            }

            SkyfallGems();
            StartCoroutine(MatchGems());
        }
    }
}