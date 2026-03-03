using UnityEngine;

[CreateAssetMenu(menuName = "Habilidades/Absorver")]
public class HabilidadeAbsorver : HabilidadeBase
{
    public int quantidadeAbsorvida = 100;

    public override void AtivarHabilidade(CardDisplay usuario, CardDisplay alvo)
    {
        // Aqui fica a lógica SÓ do Absorvel
        Debug.Log($"🌀 {usuario.cardData.nomeCarta} ativou ABSORVER!");

        // Lógica: Tira do inimigo, dá pra mim
        // Como não podemos alterar o CardData original (senão salva pra sempre), 
        // vamos alterar os valores temporários no CardDisplay ou criar variáveis de batalha.
        
        // Exemplo simples alterando o Display (visual) e os dados temporários:
        // Nota: Idealmente seu CardDisplay deve ter variáveis 'forcaAtual', 'magiaAtual' separadas dos dados originais.
        
        // Vamos supor que você quer alterar o valor que será usado no duelo agora:
        usuario.valorTemporarioBonus += quantidadeAbsorvida;
        alvo.valorTemporarioBonus -= quantidadeAbsorvida;

        Debug.Log($"Result: +{quantidadeAbsorvida} para {usuario.name} e -{quantidadeAbsorvida} para {alvo.name}");
    }
}