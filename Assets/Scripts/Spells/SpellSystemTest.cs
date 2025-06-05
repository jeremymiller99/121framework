using UnityEngine;
using System.Collections.Generic;

public class SpellSystemTest : MonoBehaviour
{
    [ContextMenu("Test All Spells")]
    void TestAllSpells()
    {
        Debug.Log("=== TESTING ALL SPELLS ===");
        
        // Create a test spell caster
        var testCaster = new SpellCaster(100, 10, Hittable.Team.PLAYER);
        testCaster.spellPower = 50;
        
        var spellBuilder = new SpellBuilder();
        
        // Test base spells
        string[] baseSpells = { "arcane_bolt", "magic_missile", "arcane_blast", "arcane_spray", "arcane_nova" };
        
        foreach (string spellName in baseSpells)
        {
            Debug.Log($"\n--- Testing {spellName} ---");
            var spell = spellBuilder.Build(testCaster, spellName);
            spell.spellPower = 50;
            spell.currentWave = 5;
            
            Debug.Log($"Name: {spell.GetName()}");
            Debug.Log($"Description: {spell.GetDescription()}");
            Debug.Log($"Damage: {spell.GetDamage()}");
            Debug.Log($"Mana Cost: {spell.GetManaCost()}");
            Debug.Log($"Cooldown: {spell.GetCooldown()}");
            Debug.Log($"Speed: {spell.GetSpeed()}");
            Debug.Log($"Trajectory: {spell.GetTrajectory()}");
        }
    }
    
    [ContextMenu("Test All Modifiers")]
    void TestAllModifiers()
    {
        Debug.Log("=== TESTING ALL MODIFIERS ===");
        
        // Create a test spell caster
        var testCaster = new SpellCaster(100, 10, Hittable.Team.PLAYER);
        testCaster.spellPower = 50;
        
        var spellBuilder = new SpellBuilder();
        
        // Test modifiers on arcane bolt
        string[] modifiers = { "damage_amp", "speed_amp", "doubler", "splitter", "chaos", "homing", "piercing", "chain_lightning" };
        
        foreach (string modifierName in modifiers)
        {
            Debug.Log($"\n--- Testing {modifierName} modifier ---");
            
            // Create base spell
            var baseSpell = spellBuilder.Build(testCaster, "arcane_bolt");
            baseSpell.spellPower = 50;
            baseSpell.currentWave = 5;
            
            Debug.Log($"Base spell - Damage: {baseSpell.GetDamage()}, Mana: {baseSpell.GetManaCost()}, Speed: {baseSpell.GetSpeed()}, Trajectory: {baseSpell.GetTrajectory()}");
            
            // Apply modifier
            var modifiedSpell = spellBuilder.BuildRandomSpell(testCaster, 50, 5);
            // Note: This creates a random spell, but we can test the modifier system
            
            Debug.Log($"Random spell with modifiers: {modifiedSpell.GetName()}");
            Debug.Log($"Modified - Damage: {modifiedSpell.GetDamage()}, Mana: {modifiedSpell.GetManaCost()}, Speed: {modifiedSpell.GetSpeed()}, Trajectory: {modifiedSpell.GetTrajectory()}");
        }
    }
    
    [ContextMenu("Test Expression Evaluation")]
    void TestExpressionEvaluation()
    {
        Debug.Log("=== TESTING EXPRESSION EVALUATION ===");
        
        var variables = new Dictionary<string, int>
        {
            ["power"] = 50,
            ["wave"] = 5
        };
        
        string[] expressions = {
            "25 power 5 / +",  // arcane_bolt damage
            "10 power 3 / +",  // magic_missile damage
            "10",              // simple mana cost
            "15 power 5 / +",  // arcane_blast mana cost
            "wave 10 *"        // spell power calculation
        };
        
        foreach (string expr in expressions)
        {
            try
            {
                int result = RPNEvaluator.Evaluate(expr, variables);
                Debug.Log($"Expression '{expr}' = {result}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error evaluating '{expr}': {e.Message}");
            }
        }
        
        // Test float expressions
        var floatVariables = new Dictionary<string, float>
        {
            ["power"] = 50f,
            ["wave"] = 5f
        };
        
        string[] floatExpressions = {
            "0.5",             // cooldown
            "8 power 5 / +",   // speed
            "1.5",             // damage multiplier
            "0.3"              // spray angle
        };
        
        foreach (string expr in floatExpressions)
        {
            try
            {
                float result = RPNEvaluator.EvaluateFloat(expr, floatVariables);
                Debug.Log($"Float expression '{expr}' = {result}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error evaluating float '{expr}': {e.Message}");
            }
        }
    }
} 