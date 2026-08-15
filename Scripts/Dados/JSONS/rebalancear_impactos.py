import json
import os
from pathlib import Path

def rebalancear_impacto(impacto_original, tipo_variacao):
    """
    Rebalanceia os valores de impacto para tornar o jogo mais equilibrado.
    
    Padrões:
    - ORIGINAL: Esp: -3 a +3, Irr: +1 a +4, Aud: 2 a 5
    - OMITIR: Esp: -1 a +2, Irr: -2 a +1, Aud: 1 a 3
    - MENTIR: Esp: +6 a +12, Irr: -6 a -10, Aud: 5 a 8
    - DISTORCER: Esp: -9 a -15, Irr: +10 a +14, Aud: 8 a 12
    """
    esp = impacto_original.get("VariacaoEsperanca", 0)
    irr = impacto_original.get("VariacaoIrritacao", 0)
    aud = impacto_original.get("AudienciaGanha", 0)
    
    if tipo_variacao == "ORIGINAL":
        # Reduz irritação e audiência, mantém esperança moderada
        novo_esp = max(min(esp * 0.6, 3), -3)
        novo_irr = max(min(irr * 0.5, 4), 1) if irr > 0 else irr * 0.5
        novo_aud = max(min(aud * 0.5, 5), 2)
        
    elif tipo_variacao == "OMITIR":
        # Mantém valores baixos e neutros
        novo_esp = max(min(esp * 0.7, 2), -1)
        novo_irr = max(min(irr * 0.5, 1), -2)
        novo_aud = max(min(aud * 0.6, 3), 1)
        
    elif tipo_variacao == "MENTIR":
        # Mantém esperança alta, reduz impactos negativos
        if esp > 0:
            novo_esp = max(min(esp * 0.7, 12), 6)
        else:
            novo_esp = esp * 0.5
        novo_irr = max(min(irr * 0.7, -6), -10) if irr < 0 else irr * 0.5
        novo_aud = max(min(aud * 0.6, 8), 5)
        
    elif tipo_variacao == "DISTORCER":
        # Reduz valores extremos
        novo_esp = max(min(esp * 0.6, -9), -15) if esp < 0 else esp * 0.5
        novo_irr = max(min(irr * 0.5, 14), 10) if irr > 0 else irr * 0.5
        novo_aud = max(min(aud * 0.45, 12), 8)
    else:
        # Fallback
        novo_esp = esp
        novo_irr = irr
        novo_aud = aud
    
    return {
        "VariacaoEsperanca": round(novo_esp, 1),
        "VariacaoIrritacao": round(novo_irr, 1),
        "AudienciaGanha": round(novo_aud, 1)
    }


def processar_arquivo(caminho_arquivo):
    """Processa um arquivo de notícias e rebalanceia os impactos."""
    print(f"Processando: {caminho_arquivo}")
    
    with open(caminho_arquivo, 'r', encoding='utf-8') as f:
        noticias = json.load(f)
    
    alteracoes = 0
    for noticia in noticias:
        if "Variacoes" not in noticia:
            continue
            
        for tipo_variacao, dados_variacao in noticia["Variacoes"].items():
            if "Impacto" in dados_variacao:
                impacto_original = dados_variacao["Impacto"]
                impacto_novo = rebalancear_impacto(impacto_original, tipo_variacao)
                
                # Só atualiza se houve mudança
                if impacto_novo != impacto_original:
                    dados_variacao["Impacto"] = impacto_novo
                    alteracoes += 1
    
    # Salva o arquivo atualizado
    with open(caminho_arquivo, 'w', encoding='utf-8') as f:
        json.dump(noticias, f, ensure_ascii=False, indent="\t")
    
    print(f"  ✓ {alteracoes} impactos rebalanceados")
    return alteracoes


def main():
    """Processa todos os arquivos Noticias.json."""
    pasta_jsons = Path(__file__).parent
    total_alteracoes = 0
    
    # Processa Dia_02 até Dia_07 (Dia_01 já foi feito manualmente)
    for dia in range(2, 8):
        caminho = pasta_jsons / f"Dia_{dia:02d}" / "Noticias.json"
        if caminho.exists():
            alteracoes = processar_arquivo(caminho)
            total_alteracoes += alteracoes
        else:
            print(f"⚠ Arquivo não encontrado: {caminho}")
    
    print(f"\n✓ Total: {total_alteracoes} impactos rebalanceados em 6 dias")


if __name__ == "__main__":
    main()
