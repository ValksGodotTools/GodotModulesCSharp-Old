using Godot;

namespace GodotModules
{
   public class LobbyNotifier
   {
      private RichTextLabel _chatText;
      private Dictionary<uint, UILobbyPlayerListing> _uiPlayers;
      public LobbyNotifier(RichTextLabel chatBox, Dictionary<uint, UILobbyPlayerListing> uiPlayers)
      {
         _chatText = chatBox;
         _uiPlayers = uiPlayers;
      }

      public void Print(string text)
      {
         _chatText.AddText($"{text}\n");
         _chatText.ScrollToLine(_chatText.GetLineCount() - 1);
      }

      public void Print(uint peerId, string text)
      {
         if (_uiPlayers.DoesNotHave(peerId))
            return;

         var playerName = NetworkManager.GameClient.Players[peerId];
         Print($"{playerName}: {text}");
      }
   }
}