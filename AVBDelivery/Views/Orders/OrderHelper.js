function calculateDiscount(discount) {
    discount = discount || 0;
    var discountFactor = Math.max(1.0, 1.0 - (discount / 100));
    console.log("dicount " + discount + " factor: " + discountFactor);
    return discountFactor;
}