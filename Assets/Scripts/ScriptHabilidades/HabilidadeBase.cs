using UnityEngine;
using System.Collections;

public abstract class HabilidadeBase : ScriptableObject
{
    // 1. Habilidade Instantânea (Antiga)
    public virtual void AtivarHabilidade(CardDisplay usuario, CardDisplay alvo) { }

    // 2. Habilidade de Combate (Pausa o duelo)
    public virtual IEnumerator AtivarHabilidadeCoroutine(CardDisplay usuario, CardDisplay alvo)
    {
        AtivarHabilidade(usuario, alvo); 
        yield break; 
    }

    // 3. Habilidade de Entrada em Campo (A que a Possessão precisa!)
    public virtual IEnumerator AoEntrarEmCampoCoroutine(CardDisplay usuario)
    {
        yield break; 
    }
}