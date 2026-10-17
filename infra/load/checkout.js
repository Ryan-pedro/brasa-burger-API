// Teste de carga do checkout com k6 (https://k6.io). Rode contra a homologação, nunca contra a produção:
//   k6 run -e BASE_URL=https://pedidos.homologacao.exemplo.com.br infra/load/checkout.js
// Perfil: pico de sexta à noite, 30 clientes simultâneos montando e enviando pedidos por 5 minutos.
// O k6 sai de um único endereço: durante o teste, suba RateLimits__OrdersPerMinute e RateLimits__QuotesPerMinute na homologação.
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '1m', target: 30 },
    { duration: '3m', target: 30 },
    { duration: '1m', target: 0 },
  ],
  thresholds: {
    http_req_failed: ['rate<0.01'],
    'http_req_duration{name:menu}': ['p(95)<300'],
    'http_req_duration{name:quote}': ['p(95)<500'],
    'http_req_duration{name:order}': ['p(95)<800'],
  },
};

const baseUrl = __ENV.BASE_URL;
const json = { headers: { 'Content-Type': 'application/json' } };

export default function () {
  const menu = http.get(`${baseUrl}/api/v1/store/menu`, { tags: { name: 'menu' } });
  check(menu, { 'cardápio carregou': (response) => response.status === 200 });

  const product = menu.json('categories.0.products.0');
  if (!product) {
    return;
  }

  const items = [{ productId: product.id, quantity: 1, optionIds: [] }];
  const quote = http.post(`${baseUrl}/api/v1/store/orders/quote`, JSON.stringify({ type: 'Pickup', items }), { ...json, tags: { name: 'quote' } });
  check(quote, { 'cotação respondeu': (response) => response.status === 200 });
  sleep(2);

  const order = http.post(
    `${baseUrl}/api/v1/store/orders`,
    JSON.stringify({
      type: 'Pickup',
      items,
      customer: { name: 'Teste de carga' },
      paymentMethod: 'Cash',
      expectedTotalInCents: quote.json('totalInCents'),
    }),
    { headers: { ...json.headers, 'Idempotency-Key': `${__VU}-${__ITER}-${Date.now()}` }, tags: { name: 'order' } },
  );
  check(order, { 'pedido criado': (response) => response.status === 201 });
  sleep(5);
}
