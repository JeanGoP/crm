// API/storage retain their historical meaning: total down payment and amount paid now.
export function isQuoteBundle(items: { productId: string }[], products: { id: string; category: string }[], categories: { name: string; active: boolean; quoteAsBundle: boolean }[]) {
  const selected = items.map(item => products.find(p => p.id === item.productId));
  const category = selected[0]?.category?.trim().toLowerCase();
  return items.length > 0 && !!category && selected.every(p => p?.category?.trim().toLowerCase() === category)
    && categories.some(c => c.active && c.quoteAsBundle && c.name.trim().toLowerCase() === category);
}

export function quotePayments(initial: number, extra: number, scheduled: number = 0) {
  return {
    downPayment: Number(initial) + Number(extra),
    initialPaymentPaidToday: Number(initial),
    extraPayment: Number(extra),
    balance: Math.max(Number(extra) - scheduled, 0),
    excess: Math.max(scheduled - Number(extra), 0)
  };
}
