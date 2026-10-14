# UI Template KIT

Open **Tools > UI Template Maker**, or right-click the Hierarchy and choose
**UI Templates > Toggle Group - Independent / Exclusive**.

The window groups settings into collapsible Layout & Selection, Toggles, Visual,
and Transition sections. Transition details start collapsed. Initially Active
lists toggle names (and None when allowed). The Create button and missing-sprite
messages stay in a fixed footer while settings scroll.

Select an object under a Canvas to choose the insertion parent. Otherwise the
tool reuses an active Canvas in the current scene or creates one. Missing scene
EventSystem and Canvas GraphicRaycaster are supplied. Creation supports Undo.
In Prefab Mode, select an object under a Canvas; no scene EventSystem is added.

- Independent: toggles can be active together.
- Exclusive: native ToggleGroup turns the previous toggle off automatically.
- Allow switch off: permits no active toggle in Exclusive mode.
- Configure count, labels, horizontal/vertical layout, starting selection and colors.
- Visual Mode: Color Palette uses default/active colors; Sprite Image uses Default
  Sprite and Active Sprite with white tint to preserve source colors. Both sprites
  are required. Sprites with borders use Sliced automatically. Active is layered
  over Background using the native Toggle graphic.
- Transition independently selects native Color Tint or Sprite Swap on Background.
  Configure normal/highlighted/pressed/selected/disabled colors or sprites. Sprite
  Swap requires a Normal Sprite or a Default Sprite from Sprite Image mode; empty
  state sprites fall back to normal. Selected is UI focus, separate from Is On.
- Width and Height set each toggle's RectTransform size (default 200 x 52).
  Both layout directions default Control Child Size and Child Force Expand off.
- Alignment follows Direction: Horizontal offers Left / Center / Right; Vertical
  offers Top / Center / Bottom. Horizontal groups extend right from Left or left
  from Right. Vertical groups extend downward from Top or upward from Bottom.
  Center keeps the group centered as children are duplicated.
  Group anchors and pivot follow this alignment. A ContentSizeFitter resizes the
  group along its stacking axis when children are added or removed.
- Toggle Name sets each hierarchy object's name independently from Label Text.
- Disable Use Button Labels to omit Label children entirely; no TMP font is needed
  for generation without labels. Names and label edits are retained when changing count.
- Initial selection is zero-based; -1 means none when allowed. Additional initial
  selections in Independent mode can be set on the created Toggle components.

Generated structure: group > Toggle N > Background > Active, with
Label alongside Background. All components and references are editable.
Active is the native Toggle graphic, so clicking, keyboard navigation,
and changes through Toggle.isOn update its visibility without a custom runtime script.
Wire game logic through each Toggle's On Value Changed event. Save the result as
a prefab by dragging the group from Hierarchy into the Project window.

Requires uGUI and a TMP default font / TMP Essential Resources. Existing scenes
are marked dirty after insertion; the tool does not save or replace them.
