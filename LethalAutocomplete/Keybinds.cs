using UnityEngine.InputSystem;
using LethalCompanyInputUtils.Api;

namespace LethalAutocomplete
{
    internal class Keybinds : LcInputActions
    {
        public InputAction AutocompleteAction => Asset["Autocomplete"];
        public InputAction HistoryNextAction => Asset["HistoryNext"];
        public InputAction HistoryPrevAction => Asset["HistoryPrev"];

        public override void CreateInputActions(in InputActionMapBuilder builder)
        {
            base.CreateInputActions(builder);
            builder.NewActionBinding()
                .WithActionId("Autocomplete")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(AutocompleteManager.autocompleteKey)
                .WithBindingName("Autocomplete Key")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("HistoryNext")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(AutocompleteManager.historyNextKey)
                .WithBindingName("HistoryNext Key")
                .Finish();
            builder.NewActionBinding()
                .WithActionId("HistoryPrev")
                .WithActionType(InputActionType.Button)
                .WithKbmPath(AutocompleteManager.historyPrevKey)
                .WithBindingName("HistoryPrev Key")
                .Finish();
        }
    }
}