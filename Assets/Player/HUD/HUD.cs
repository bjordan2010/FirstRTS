using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class HUD : MonoBehaviour
{
    public Texture2D activeCursor;
    public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
    public Texture2D[] moveCursors, attackCursors, harvestCursors;

    private const int SELECTION_NAME_HEIGHT = 20;
    public GUISkin resourceSkin, ordersSkin, selectBoxSkin, mouseCursorSkin;
    private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
    private Player player;

    private CursorState activeCursorState;
    private int currentFrame = 0;

    private Dictionary<ResourceType, int> resources, resourceLimits;
    private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
    public Texture2D[] resourceTextures;
    private Dictionary<ResourceType, Texture2D> resourceImages;

    private WorldObject lastSelection;
    private float sliderValue;

    public Texture2D buttonHover, buttonClick, buildFrame, buildMask;
    private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64, BUILD_IMAGE_PADDING = 8;
    private int buildAreaHeight = 0;
    private const int BUTTON_SPACING = 7, SCROLL_BAR_WIDTH = 22;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.root.GetComponent<Player>();
        ResourceManager.StoreSelectBoxItems(selectBoxSkin);
        SetCursorState(CursorState.Select);
        resources = new Dictionary<ResourceType, int>();
        resourceLimits = new Dictionary<ResourceType, int>();
        resourceImages = new Dictionary<ResourceType, Texture2D>();
        for (int i = 0; i < resourceTextures.Length; i++) {
            switch(resourceTextures[i].name) {
                case "Money":
                    resourceImages.Add(ResourceType.Money, resourceTextures[i]);
                    resources.Add(ResourceType.Money, 0);
                    resourceLimits.Add(ResourceType.Money, 0);
                    break;
                case "Power":
                    resourceImages.Add(ResourceType.Power, resourceTextures[i]);
                    resources.Add(ResourceType.Power, 0);
                    resourceLimits.Add(ResourceType.Power, 0);
                    break;
                default: break;
            }
        }
        buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;
    }

    void OnGUI()
    {
        if (player && player.human) {
            DrawOrdersBar();
            DrawResourceBar();
            DrawMouseCursor();
        }
    }

    // Public Methods
    // returns true if player does NOT have the mouse pointer on the HUD
    public bool MouseInBounds() {
        Vector3 mousePos = Input.mousePosition;
        bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
        bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
        return insideWidth && insideHeight;
    }

    public Rect GetPlayingArea() {
        return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width-ORDERS_BAR_WIDTH,
            Screen.height-RESOURCE_BAR_HEIGHT);
    }

    public void DrawMouseCursor() {
        bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && 
            activeCursorState != CursorState.PanUp;
        if(mouseOverHud) {
            Cursor.visible = true;
        } else {
            Cursor.visible = false;
            GUI.skin = mouseCursorSkin;
            GUI.BeginGroup(new Rect(0,0,Screen.width,Screen.height));
            UpdateCursorAnimation();
            Rect cursorPosition = GetCursorDrawPosition();
            GUI.Label(cursorPosition, activeCursor);
            GUI.EndGroup();
        }
    }

    public void UpdateCursorAnimation() {
        if(activeCursorState == CursorState.Move) {
            currentFrame = (int)Time.time % moveCursors.Length;
            activeCursor = moveCursors[currentFrame];
        } else if(activeCursorState == CursorState.Attack) {
            currentFrame = (int)Time.time % attackCursors.Length;
            activeCursor = attackCursors[currentFrame];
        } else if(activeCursorState == CursorState.Harvest) {
            currentFrame = (int)Time.time % harvestCursors.Length;
            activeCursor = harvestCursors[currentFrame];
        }
    }

    public Rect GetCursorDrawPosition() {
        float leftPos = Input.mousePosition.x;
        float topPos = Screen.height - Input.mousePosition.y;
        if(activeCursorState == CursorState.PanRight) leftPos = Screen.width - activeCursor.width;
        else if(activeCursorState == CursorState.PanDown) topPos = Screen.height - activeCursor.height;
        else if(activeCursorState == CursorState.Move || activeCursorState == CursorState.Select ||
            activeCursorState == CursorState.Harvest) {
                topPos -= activeCursor.height / 2;
                leftPos -= activeCursor.width / 2;
            }
        return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
    }

    public void SetCursorState(CursorState newState) {
        activeCursorState = newState;
        switch (newState)
        {
            case CursorState.Select:
                activeCursor = selectCursor;
                break;
            case CursorState.Attack:
                currentFrame = (int)Time.time % attackCursors.Length;
                activeCursor = attackCursors[currentFrame];
                break;
            case CursorState.Harvest:
                currentFrame = (int)Time.time % harvestCursors.Length;
                activeCursor = harvestCursors[currentFrame];
                break;
            case CursorState.Move:
                currentFrame = (int)Time.time % moveCursors.Length;
                activeCursor = moveCursors[currentFrame];
                break;
            case CursorState.PanLeft:
                activeCursor = leftCursor;
                break;
            case CursorState.PanRight:
                activeCursor = rightCursor;
                break;
            case CursorState.PanUp:
                activeCursor = upCursor;
                break;
            case CursorState.PanDown:
                activeCursor = downCursor;
                break;
            default: break;
        }
    }

    public void SetResourceValues(Dictionary<ResourceType, int> resources, Dictionary<ResourceType, int> resourceLimits) {
        this.resources = resources;
        this.resourceLimits = resourceLimits;
    }
    // End Public Methods

    // Private Methods
    private void DrawOrdersBar() {
        GUI.skin = ordersSkin;
        GUI.BeginGroup(new Rect(Screen.width-ORDERS_BAR_WIDTH-BUILD_IMAGE_WIDTH,RESOURCE_BAR_HEIGHT,
            ORDERS_BAR_WIDTH+BUILD_IMAGE_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT));
        GUI.Box(new Rect(BUILD_IMAGE_WIDTH+SCROLL_BAR_WIDTH,0,ORDERS_BAR_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT),"");
        string selectionName = "";
        if(player && player.SelectedObject) {
            selectionName = player.SelectedObject.objectName;
        }
        if(!selectionName.Equals("")) {
            int leftPos = BUILD_IMAGE_WIDTH+SCROLL_BAR_WIDTH/2;
            int topPos = buildAreaHeight + BUTTON_SPACING;
            GUI.Label(new Rect(leftPos,topPos,ORDERS_BAR_WIDTH,SELECTION_NAME_HEIGHT), selectionName);
        }
        if (player && player.SelectedObject && player.SelectedObject.IsOwnedBy(player)) {
            if(lastSelection && lastSelection != player.SelectedObject) sliderValue = 0.0f;
            DrawActions(player.SelectedObject.GetActions());
            lastSelection = player.SelectedObject;
            Building selectedBuilding = lastSelection.GetComponent<Building>();
            if(selectedBuilding) {
                DrawBuildQueue(selectedBuilding.getBuildQueueValues(), selectedBuilding.getBuildPercentage());
            }
        }
        GUI.EndGroup();
    }

    private void DrawBuildQueue(string[] buildQueue, float buildPercentage) {
        for (int i = 0; i < buildQueue.Length; i++) {
            float topPos = i * BUILD_IMAGE_HEIGHT - (i+1) * BUILD_IMAGE_PADDING;
            Rect buildPos = new Rect(BUILD_IMAGE_PADDING, topPos, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
            GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(buildQueue[i]));
            GUI.DrawTexture(buildPos, buildFrame);
            float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
            float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
            if(i==0) {
                topPos += height * buildPercentage;
                height *= (1 - buildPercentage);
            }
            GUI.DrawTexture(new Rect(2 * BUILD_IMAGE_PADDING, topPos, width, height), buildMask);
        }
    }

    private void DrawResourceBar() {
        GUI.skin = resourceSkin;
        GUI.BeginGroup(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT));
        GUI.Box(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT),"");
        int topPos = 4, iconLeft = 4, textLeft = 20;
        DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);
        iconLeft += TEXT_WIDTH;
        textLeft += TEXT_WIDTH;
        DrawResourceIcon(ResourceType.Power, iconLeft, textLeft, topPos);
        GUI.EndGroup();
    }

    private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos) {
        Texture2D icon = resourceImages[type];
        string text = resources[type].ToString() + "/" + resourceLimits[type].ToString();
        GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
        GUI.Label(new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
    }

    private void DrawActions(string[] actions) {
        GUIStyle buttons = new GUIStyle();
        buttons.hover.background = buttonHover;
        buttons.active.background = buttonClick;
        GUI.skin.button = buttons;
        int numActions = actions.Length;
        GUI.BeginGroup(new Rect(BUILD_IMAGE_WIDTH,0,ORDERS_BAR_WIDTH, buildAreaHeight));
        if(numActions >= MaxNumRows(buildAreaHeight)) DrawSlider(buildAreaHeight, numActions / 2.0f);
        for (int i = 0; i < numActions; i++) {
            int col = i % 2;
            int row = i / 2;
            Rect pos = GetButtonPos(row, col);
            Texture2D action = ResourceManager.GetBuildImage(actions[i]);
            if (action) {
                if(GUI.Button(pos, action)) {
                    if(player.SelectedObject) player.SelectedObject.PerformAction(actions[i]);
                }
            }
        }
        GUI.EndGroup();
    }

    private int MaxNumRows(int areaHeight) {
        return areaHeight / BUILD_IMAGE_HEIGHT;
    }

    private Rect GetButtonPos(int row, int col) {
        int left = SCROLL_BAR_WIDTH + col * BUILD_IMAGE_WIDTH;
        float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
        return new Rect(left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
    }

    private void DrawSlider(int groupHeight, float numRows) {
        sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight), sliderValue, 0.0f, numRows - MaxNumRows(groupHeight));
    }

    private Rect GetScrollPos(int groupHeight) {
        return new Rect(BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, groupHeight - 2 * BUTTON_SPACING);
    }
    // End Private Methods
}
