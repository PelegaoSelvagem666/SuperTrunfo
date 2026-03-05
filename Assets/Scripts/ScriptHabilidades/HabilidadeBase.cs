using UnityEngine;
using System.Collections;

public abstract class HabilidadeBase : ScriptableObject
{
    // Habilidades de Combate (Absorvel, Buff, Quebra de Regra, etc)
    public virtual void AtivarHabilidade(CardDisplay usuario, CardDisplay alvo) { }
    public virtual IEnumerator AtivarHabilidadeCoroutine(CardDisplay usuario, CardDisplay alvo)
    {
        AtivarHabilidade(usuario, alvo); 
        yield break; 
    }

    // --- NOVO: Habilidades de Entrada em Campo (Possessão) ---
    public virtual IEnumerator AoEntrarEmCampoCoroutine(CardDisplay usuario)
    {
        // Por padrão não faz nada, só as cartas que sobrescreverem isso vão pausar o jogo
        yield break; 
    }
}