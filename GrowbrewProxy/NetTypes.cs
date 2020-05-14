using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowbrewProxy
{
    public class NetTypes
    {
        public enum PacketTypes
        {
            PLAYER_LOGIC_UPDATE = 0,
            CALL_FUNCTION,
            UPDATE_STATUS,
            TILE_CHANGE_REQ,
            LOAD_MAP,
            TILE_EXTRA,
            TILE_EXTRA_MULTI,
            TILE_ACTIVATE,
            APPLY_DMG,
            INVENTORY_STATE,
            ITEM_ACTIVATE,
            ITEM_ACTIVATE_OBJ,
            UPDATE_TREE,
            MODIFY_INVENTORY_ITEM,
            MODIFY_ITEM_OBJ,
            APPLY_LOCK,
            UPDATE_ITEMS_DATA,
            PARTICLE_EFF,
            ICON_STATE,
            ITEM_EFF,
            SET_CHARACTER_STATE,
            PING_REPLY,
            PING_REQ,
            PLAYER_HIT,
            APP_CHECK_RESPONSE,
            APP_INTEGRITY_FAIL,
            DISCONNECT,
            BATTLE_JOIN,
            BATTLE_EVENT,
            USE_DOOR,
            PARENTAL_MSG,
            GONE_FISHIN,
            STEAM,
            PET_BATTLE,
            NPC,
            SPECIAL,
            PARTICLE_EFFECT_V2,
            ARROW_TO_ITEM,
            TILE_INDEX_SELECTION,
            UPDATE_PLAYER_TRIBUTE
        };

        public enum NetMessages
        {
            UNKNOWN = 0,
            SERVER_HELLO,
            GENERIC_TEXT,
            GAME_MESSAGE,
            GAME_PACKET,
            ERROR,
            TRACK,
            LOG_REQ,
            LOG_RES
        };

    }
}
