using UnityEngine;

[CreateAssetMenu(fileName = "Habilidade Absorver", menuName = "SuperTrunfo/Habilidades/Absorver")]
public class HabilidadeAbsorver : HabilidadeBase
{
    [Header("Configuração")]
    public int intencaoDeRoubo = 100; // Quanto ele TENTA roubar

    public override void AtivarHabilidade(CardDisplay usuario, CardDisplay alvo)
    {
        // 1. Pega o GameManager
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        
        if (gm == null || string.IsNullOrEmpty(gm.atributoEmDisputa)) return;

        string atributo = gm.atributoEmDisputa;
        Debug.Log($"🌀 {usuario.cardData.nomeCarta} tentando roubar atributos em {atributo}!");

        // 2. Descobre quanto o inimigo tem DE VERDADE agora
        int baseInimigo = gm.PegarValorAtributo(alvo.cardData, atributo);
        
        // O inimigo pode ter bonus ou penalidades anteriores
        int totalInimigo = baseInimigo + alvo.valorTemporarioBonus; 

        // Se o total for menor que 0 (por causa de outro debuff), consideramos 0
        if (totalInimigo < 0) totalInimigo = 0;

        // 3. A Lógica da Troca Justa
        // Eu só posso roubar o que ele tem.
        // Se quero 100, mas ele tem 40 -> Roubo 40.
        int valorRealRoubado = Mathf.Min(intencaoDeRoubo, totalInimigo);

        if (valorRealRoubado > 0)
        {
            // Aplica a transferência
            usuario.valorTemporarioBonus += valorRealRoubado;
            alvo.valorTemporarioBonus -= valorRealRoubado;

            Debug.Log($"✅ Absorvido: {valorRealRoubado}. (Inimigo tinha {totalInimigo})");
        }
        else
        {
            Debug.Log("🚫 Inimigo já está zerado. Nada para absorver.");
        }
        
        // NOTA: Não precisamos checar o limite de 1000 aqui, 
        // porque o GameManager fará o 'Mathf.Clamp(0, 1000)' no final do turno para ambos.
    }
}