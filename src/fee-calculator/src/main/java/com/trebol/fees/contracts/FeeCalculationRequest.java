package com.trebol.fees.contracts;

import jakarta.validation.constraints.DecimalMin;
import jakarta.validation.constraints.Digits;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Pattern;
import java.math.BigDecimal;

public record FeeCalculationRequest(
        @NotNull
        @DecimalMin(value = "0.0", inclusive = false)
        @Digits(integer = Integer.MAX_VALUE, fraction = 2)
        BigDecimal amount,
        @NotBlank
        @Pattern(regexp = "WEB|MOBILE|BRANCH")
        String channel) {
}