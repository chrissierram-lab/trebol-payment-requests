package com.trebol.fees.contracts;

import java.math.BigDecimal;

public record FeeCalculationResponse(BigDecimal fee, BigDecimal total) {
}