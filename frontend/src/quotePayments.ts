// API/storage retain their historical meaning: total down payment and amount paid now.
export function quotePayments(initial: number, extra: number, scheduled: number = 0) {
  return {
    downPayment: Number(initial) + Number(extra),
    initialPaymentPaidToday: Number(initial),
    extraPayment: Number(extra),
    balance: Math.max(Number(extra) - scheduled, 0),
    excess: Math.max(scheduled - Number(extra), 0)
  };
}
