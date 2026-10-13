using Brasa.Domain.Catalog;

namespace Brasa.Infrastructure.Persistence;

/// <summary>
/// Cardápio de demonstração, o mesmo do protótipo do Storefront.
/// As imagens apontam para os arquivos em <c>projects/storefront/public/img/produtos</c> do front-end.
/// </summary>
internal static class DemoMenu
{
    public static void AddTo(AppDbContext db)
    {
        var c0 = Add(db, "Hambúrgueres", 0);
        Item(db, c0, "X-Bacon", "Pão brioche, hambúrguer artesanal 150g, queijo prato, bacon crocante, alface e tomate.", 24.9m, null, "img/produtos/b_xbacon.svg", 0, featured: true, available: true);
        Item(db, c0, "Clássico da Brasa", "Blend 150g na brasa, cheddar, picles, cebola roxa, alface e molho da casa.", 22.9m, null, "img/produtos/b_classico.svg", 1, featured: false, available: true);
        Item(db, c0, "Duplo Smash", "Dois smash de 90g com crosta, cheddar em dobro e cebola na chapa.", 29.9m, null, "img/produtos/b_smash.svg", 2, featured: true, available: true);
        Item(db, c0, "Brasa BBQ", "Blend 180g, provolone, onion rings crocantes e barbecue defumado.", 31.9m, null, "img/produtos/b_bbq.svg", 3, featured: false, available: true);
        Item(db, c0, "Chicken Crispy", "Sobrecoxa empanada e crocante, coleslaw, alface e maionese de alho.", 26.9m, null, "img/produtos/b_chicken.svg", 4, featured: false, available: true);
        Item(db, c0, "Veggie da Horta", "Hambúrguer de grão-de-bico e beterraba, queijo coalho, tomate e rúcula.", 25.9m, null, "img/produtos/b_veggie.svg", 5, featured: false, available: true);
        Item(db, c0, "Cheddar Melt", "Blend 150g, cheddar cremoso em dobro e bacon, no pão brioche.", 27.9m, null, "img/produtos/b_melt.svg", 6, featured: false, available: true);
        Item(db, c0, "Brasa Monstro", "Três blends de 150g, três queijos, bacon e alface. Para os fortes.", 39.9m, null, "img/produtos/b_monstro.svg", 7, featured: false, available: false);

        var c1 = Add(db, "Combos", 1);
        Item(db, c1, "Combo da Semana", "X-Bacon + batata frita média + bebida 350ml.", 32.9m, null, "img/produtos/combo_semana.svg", 0, featured: true, available: true);
        Item(db, c1, "Combo Smash", "Duplo Smash + batata frita média + bebida 350ml.", 42.9m, null, "img/produtos/combo_smash.svg", 1, featured: false, available: true);
        Item(db, c1, "Combo Chicken", "Chicken Crispy + batata frita média + bebida 350ml.", 38.9m, null, "img/produtos/combo_chicken.svg", 2, featured: false, available: true);
        Item(db, c1, "Combo Casal", "X-Bacon + Clássico da Brasa + batata grande + 2 bebidas 350ml.", 74.9m, null, "img/produtos/combo_casal.svg", 3, featured: false, available: true);

        var c2 = Add(db, "Hot Dogs", 2);
        Item(db, c2, "Dog da Brasa", "Salsicha defumada na brasa, mostarda, ketchup e batata palha.", 18.9m, null, "img/produtos/dog_classico.svg", 0, featured: false, available: true);
        Item(db, c2, "Dog Cheddar Bacon", "Salsicha defumada, cheddar cremoso e bacon crocante.", 21.9m, null, "img/produtos/dog_cheddar.svg", 1, featured: false, available: true);

        var c3 = Add(db, "Porções", 3);
        Item(db, c3, "Batata Frita Grande", "Batatas crocantes com sal e páprica. Serve 2 pessoas.", 19.9m, null, "img/produtos/fritas.svg", 0, featured: true, available: true);
        Item(db, c3, "Batata Cheddar & Bacon", "Batata frita coberta com cheddar cremoso, bacon e cebolinha.", 27.9m, null, "img/produtos/fritas_cheddar.svg", 1, featured: false, available: true);
        Item(db, c3, "Onion Rings", "Anéis de cebola empanados, com molho barbecue.", 22.9m, null, "img/produtos/onion_rings.svg", 2, featured: false, available: true);
        Item(db, c3, "Frango Crocante", "Tiras de frango empanadas com molho de mostarda e mel.", 29.9m, null, "img/produtos/frango.svg", 3, featured: false, available: true);

        var c4 = Add(db, "Sobremesas", 4);
        Item(db, c4, "Brownie com Sorvete", "Brownie quente de chocolate, sorvete de creme e calda.", 16.9m, null, "img/produtos/brownie.svg", 0, featured: true, available: true);
        Item(db, c4, "Churros com Doce de Leite", "Três churros com açúcar e canela e doce de leite para mergulhar.", 12.9m, null, "img/produtos/churros.svg", 1, featured: false, available: true);
        Item(db, c4, "Sundae de Caramelo", "Sorvete de creme, calda de caramelo e confeitos.", 13.9m, null, "img/produtos/sundae.svg", 2, featured: false, available: true);

        var c5 = Add(db, "Bebidas", 5);
        Item(db, c5, "Coca-Cola", "Lata gelada.", 6.0m, "350ml", "img/produtos/coca.svg", 0, featured: true, available: true);
        Item(db, c5, "Guaraná", "Lata gelada.", 6.0m, "350ml", "img/produtos/guarana.svg", 1, featured: false, available: true);
        Item(db, c5, "Suco de laranja", "Natural, espremido na hora.", 9.9m, "500ml", "img/produtos/suco_laranja.svg", 2, featured: false, available: true);
        Item(db, c5, "Suco de maracujá", "Natural, feito com a fruta.", 9.9m, "500ml", "img/produtos/suco_maracuja.svg", 3, featured: false, available: true);
        Item(db, c5, "Água mineral", "Sem gás.", 4.0m, "500ml", "img/produtos/agua.svg", 4, featured: false, available: true);
        Item(db, c5, "Água com gás", "Com gás.", 4.5m, "500ml", "img/produtos/agua_gas.svg", 5, featured: false, available: false);
        Item(db, c5, "Milk-shake de chocolate", "Sorvete de chocolate batido, chantilly e calda.", 14.9m, "500ml", "img/produtos/shake_choc.svg", 6, featured: true, available: true);
        Item(db, c5, "Milk-shake de morango", "Sorvete de morango batido, chantilly e calda.", 14.9m, "500ml", "img/produtos/shake_morango.svg", 7, featured: false, available: true);
        Item(db, c5, "Chá gelado de limão", "Chá preto gelado com limão.", 7.9m, "400ml", "img/produtos/cha.svg", 8, featured: false, available: true);
        Item(db, c5, "Limonada suíça", "Limão batido com leite condensado e gelo.", 10.9m, "500ml", "img/produtos/limonada.svg", 9, featured: false, available: true);
    }

    private static Category Add(AppDbContext db, string name, int sortOrder)
    {
        var category = new Category(name, sortOrder);
        db.Categories.Add(category);
        return category;
    }

    private static void Item(
        AppDbContext db,
        Category category,
        string name,
        string description,
        decimal price,
        string? size,
        string imageUrl,
        int sortOrder,
        bool featured,
        bool available)
    {
        var product = new Product(category.Id, name, description, price, size, imageUrl, sortOrder);
        product.SetFeatured(featured);
        product.SetAvailability(available);
        db.Products.Add(product);
    }
}
