using UnityEngine;
using System.Collections.Generic; 

[CreateAssetMenu(fileName = "Imune a Tipos", menuName = "SuperTrunfo/Habilidades/Invencivel Contra Tipos")]
public class HabilidadeInvencivelTipo : HabilidadeBase
{
    [Header("Configuração da Imunidade")]
    [Tooltip("Adicione todos os tipos que esta carta derrota automaticamente")]
    public List<CardType> tiposImunes = new List<CardType>(); 

    public override void AtivarHabilidade(CardDisplay usuario, CardDisplay alvo)
    {
        // Pega o tipo do inimigo diretamente no formato original dele
        CardType tipoDoAlvo = alvo.cardData.tipo; 

        // Verifica se o tipo do inimigo está dentro da nossa lista de imunidades
        if (tiposImunes.Contains(tipoDoAlvo))
        {
            Debug.Log($"🛡️ {usuario.cardData.nomeCarta} ativou INVENCIBILIDADE! O inimigo é do tipo {tipoDoAlvo} e foi esmagado.");

            // Aplica a vitória matemática 
            usuario.valorTemporarioBonus += 2000;
            alvo.valorTemporarioBonus -= 2000;
        }
        else
        {
            Debug.Log($"⚠️ O inimigo é do tipo {tipoDoAlvo}. A imunidade não cobre esse tipo.");
        }
    }
}