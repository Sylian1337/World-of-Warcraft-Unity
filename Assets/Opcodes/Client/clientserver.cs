using Mirror;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ClientNetworkManager : MonoBehaviour
{
    private ClientOpcodeHandler opcodeHandler;

    void Start()
    {
        opcodeHandler = new ClientOpcodeHandler();

        // Register opcode handlers
        opcodeHandler.RegisterHandler(Opcodes.SMSG_SPELL_START,         HandleSpellStart);
        opcodeHandler.RegisterHandler(Opcodes.SMSG_SPELL_GO,            HandleSpellGo);
        opcodeHandler.RegisterHandler(Opcodes.SMSG_SEND_COMBAT_TEXT,    HandleCombatText);
        opcodeHandler.RegisterHandler(Opcodes.SMSG_UPDATE_STAT,         HandleUpdateStat);
        opcodeHandler.RegisterHandler(Opcodes.SMSG_AURA_UPDATE,         HandleAuraUpdate);
        opcodeHandler.RegisterHandler(Opcodes.SMSG_CAST_CANCELED,       HandleCancelCast);
        // Register the OpcodeMessage handler on the client
        NetworkClient.RegisterHandler<OpcodeMessage>(OnOpcodeMessageReceived);
    }

    private void OnOpcodeMessageReceived(OpcodeMessage msg)
    {
        // Handle the opcode using the registered handler
        opcodeHandler.HandleOpcode(msg.opcode, new NetworkReader(msg.payload));
    }

    private void HandleSpellStart(NetworkReader reader)
    {
        // Deserialize each field in the same order as they were serialized
        int castFlags = reader.ReadInt();
        NetworkIdentity casterIdentity = reader.ReadNetworkIdentity();
        NetworkIdentity targetIdentity = reader.ReadNetworkIdentity();
        float castTime = reader.ReadFloat();
        float gcd = reader.ReadFloat();
        int spellId = reader.ReadInt();
        bool animationEnabled = reader.ReadBool();
        bool isSpellQueueSpell = reader.ReadBool();
        Vector3 aoePosition = reader.ReadVector3();
        bool voc = reader.ReadBool();

        // Now you have all the deserialized data
        Debug.Log($"Received spell start for spell ID {spellId}");

        // Retrieve the Unit component associated with the caster
        Unit caster = casterIdentity.GetComponent<Unit>();

        if (casterIdentity.netId == NetworkClient.localPlayer.netId)
        {
            UIHandler.Instance.StartCast(castTime, GetSpellNameById(spellId));

            // ActionBar stuff
            if (gcd > 0)
            {
                UIHandler.Instance.StartGlobalCooldown(gcd);
            }
        }

    }

    private string GetSpellNameById(int spellId)
    {
        // Replace with your actual logic to retrieve the spell name
        SpellInfo spellInfo = SpellDataHandler.Instance.Spells.FirstOrDefault(spell => spell.Id == spellId);
        return spellInfo != null ? spellInfo.Name : "Unknown Spell";
    }

    private void HandleSpellGo(NetworkReader reader)
    {
        // Deserialize each field in the same order as they were serialized
        int castFlags = reader.ReadInt();
        NetworkIdentity casterIdentity = reader.ReadNetworkIdentity();
        NetworkIdentity targetIdentity = reader.ReadNetworkIdentity();
        float castTime = reader.ReadFloat();
        int spellId = reader.ReadInt();
        float speed = reader.ReadFloat();
        float spellTime = reader.ReadFloat();
        bool animationEnabled = reader.ReadBool();
        Vector3 aoePosition = reader.ReadVector3();
        int manaCost = reader.ReadInt(); // Assuming mana cost is an int, adjust if needed
        bool voc = reader.ReadBool();
        bool isToggled = reader.ReadBool();

        // Now you have all the deserialized data
        Debug.Log($"Received spell go for spell ID {spellId}");

        // Retrieve the Unit component associated with the caster
        Unit caster = casterIdentity.GetComponent<Unit>();
        Unit target = targetIdentity.GetComponent<Unit>();

        if (casterIdentity.netId == NetworkClient.localPlayer.netId)
        {
                
        }

        if (spellTime > 0)
        {

            Transform targetTransform = target.transform;
            Transform casterTransform = caster.transform;

            if (!targetTransform)
                Debug.LogError("No transform found for Target!");

            VFXManager.Instance.CastSpell(spellId, speed, casterTransform, targetTransform);
        }

        // Implement additional logic here, such as starting animations, reducing mana, etc.
    }

    private void HandleUpdateStat(NetworkReader reader)
    {
        NetworkIdentity networkIdentity = reader.ReadNetworkIdentity();
        string statChanged = reader.ReadString();
        float statValue = reader.ReadFloat();
        float maxStatValue = reader.ReadFloat();

        Unit sender = networkIdentity.GetComponent<Unit>();

        GameObject canvas = GameObject.FindWithTag("Canvas");

        if (!canvas)
            Debug.LogError("CANNOT FIND CANVAS!");

        if (networkIdentity.netId == NetworkClient.localPlayer.netId)
        {
            if (statChanged == "health")
            {
                UIHandler.Instance.UpdateHealth(statValue, maxStatValue);
            }
        }
    }

    private void HandleAuraUpdate(NetworkReader reader)
    {
        NetworkIdentity casterIdentity = reader.ReadNetworkIdentity();
        NetworkIdentity targetIdentity = reader.ReadNetworkIdentity();
        

        Unit caster = casterIdentity.GetComponent<Unit>();
        Unit target = targetIdentity.GetComponent<Unit>();

        if (casterIdentity.netId == NetworkClient.localPlayer.netId)
        {
            // We are caster
        }
        else if (targetIdentity.netId == NetworkClient.localPlayer.netId)
        {
            // We are target
        }
        

        // This stuff needs to be run regardless
        
    }

    private void HandleCombatText(NetworkReader reader)
    {
        float newHealth = reader.ReadFloat();
        bool positive = reader.ReadBool();
        NetworkIdentity identity = reader.ReadNetworkIdentity();

        Unit target = identity.GetComponent<Unit>();

        if (identity.netId == NetworkClient.localPlayer.netId)
        {
            // This is working
        }
    }

    private void HandleCancelCast(NetworkReader reader)
    {
        NetworkIdentity identity = reader.ReadNetworkIdentity();
        int spellId = reader.ReadInt();
        Unit caster = identity.GetComponent<Unit>();

        if (identity.netId == NetworkClient.localPlayer.netId)
        {
            // This is working
        }
    }
}
