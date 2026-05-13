# Mobile Responsiveness Test Specification
## Discount Redemption Feature - WCAG 2.1 AA Compliance

**Last Updated**: 2025-12-08  
**Feature**: Discount Redemption Gallery & Modal  
**Minimum Target**: 320px viewport width (iPhone SE)

---

## Test Breakpoints

| Breakpoint | Device Examples | Width | Priority |
|------------|----------------|-------|----------|
| **XS (Mobile)** | iPhone SE, Galaxy Fold | 320px - 374px | P0 |
| **Small (Mobile)** | iPhone 12/13, Galaxy S21 | 375px - 639px | P0 |
| **Medium (Tablet)** | iPad Mini, Surface Duo | 640px - 1023px | P1 |
| **Large (Desktop)** | iPad Pro, Laptop | 1024px - 1279px | P1 |
| **XL (Desktop)** | Desktop, Large Monitor | 1280px+ | P2 |

---

## Test Cases

### T116.1: Layout Responsiveness (P0)

#### Discount Gallery Grid
- [ ] **320px**: Single column layout (grid-cols-1)
- [ ] **375px**: Single column with proper padding (px-4)
- [ ] **640px**: Two columns (sm:grid-cols-2)
- [ ] **1024px**: Three columns (lg:grid-cols-3)
- [ ] **1920px**: Three columns maintained (no overflow)

**Expected**: Cards stack vertically on mobile, expand to grid on larger screens

#### Discount Card Component
- [ ] **320px**: Card width 100%, min-width respected
- [ ] **320px**: All text readable (minimum 16px font size)
- [ ] **320px**: Badge text not truncated
- [ ] **320px**: Button text "Redeem Now" fully visible
- [ ] **375px**: Discount percentage (2xl) clearly visible
- [ ] **768px**: Card height consistent across row

**Expected**: Cards maintain readability and structure at all breakpoints

#### RedemptionModal
- [ ] **320px**: Modal fits viewport (sm:max-w-md respected)
- [ ] **320px**: Modal content not cut off or requiring scroll
- [ ] **320px**: Buttons stack if needed (DialogFooter responsive)
- [ ] **375px**: Cancel/Confirm buttons side-by-side
- [ ] **768px**: Modal centered with padding

**Expected**: Modal remains usable and accessible on smallest screens

---

### T116.2: Touch Target Sizes (P0) - WCAG 2.5.5

Minimum touch target: **44px × 44px**

#### DiscountCard
- [ ] "Redeem Now" button: min-height 44px, min-width 44px ✓ (w-full enforces width)
- [ ] Touch target easily tappable without zooming
- [ ] Sufficient spacing between adjacent cards (gap-4 sm:gap-6)

#### RedemptionModal
- [ ] "Cancel" button: min-height 44px ✓
- [ ] "Confirm Redemption" button: min-height 44px ✓
- [ ] "Copy Code" icon button: 44px × 44px ✓ (size="icon")
- [ ] "Done" button: min-height 44px, full width ✓
- [ ] Close X button: 44px × 44px ✓ (Dialog default)

**Expected**: All interactive elements meet 44px minimum on mobile

---

### T116.3: Typography & Readability (P0) - WCAG 1.4.4

Minimum font size: **16px** (1rem base)

#### DiscountCard Typography
- [ ] **320px**: "50% OFF" (text-2xl = 24px) readable ✓
- [ ] **320px**: "Code: PROMO50" (text-xs = 12px) readable
- [ ] **320px**: "Required Points: 1,000 pts" (text-sm = 14px) readable
- [ ] **320px**: Button text "Redeem Now" (base = 16px) readable ✓
- [ ] **320px**: Warning text (text-xs = 12px) readable
- [ ] **320px**: Line height sufficient (no text overlap)

#### RedemptionModal Typography
- [ ] **320px**: Modal title (default = 18px) readable ✓
- [ ] **320px**: Discount code (text-2xl = 24px) readable ✓
- [ ] **320px**: Description text (text-sm = 14px) readable
- [ ] **320px**: Button text (base = 16px) readable ✓

**Expected**: All text readable without zooming (min 12px acceptable for labels)

---

### T116.4: Horizontal Scrolling (P0) - WCAG 1.4.10

- [ ] **320px**: No horizontal scrollbar on gallery page
- [ ] **320px**: No content cut off requiring horizontal scroll
- [ ] **320px**: Modal content fits width (no horizontal scroll)
- [ ] **375px**: Discount cards don't overflow container
- [ ] **375px**: Modal buttons don't overflow footer
- [ ] **768px**: Three-column grid doesn't overflow

**Expected**: Zero horizontal scrolling at any breakpoint

---

### T116.5: Image & Icon Scaling (P1)

#### Icons in DiscountCard
- [ ] Badge icon scales appropriately (shrink-0 prevents squishing)
- [ ] Warning emoji (⚠️) displays correctly
- [ ] No icon distortion at 320px

#### Icons in RedemptionModal
- [ ] Celebration emoji (🎉) displays correctly
- [ ] Copy icon (lucide-react) scales properly
- [ ] Check icon (lucide-react) displays after copy
- [ ] Icons maintain aspect ratio on mobile

**Expected**: All icons crisp and properly sized on mobile

---

### T116.6: Form & Input Elements (P1)

#### Modal Interaction
- [ ] **320px**: Tap targets don't overlap
- [ ] **320px**: Button text not truncated
- [ ] **375px**: Focus indicators visible on tap
- [ ] **375px**: Modal backdrop properly dims background
- [ ] **768px**: Keyboard opens without breaking layout (if applicable)

**Expected**: Form elements remain functional and accessible on mobile

---

### T116.7: Orientation Support (P1) - WCAG 1.3.4

#### Portrait Orientation
- [ ] **320px portrait**: Gallery displays correctly
- [ ] **375px portrait**: Modal fits viewport
- [ ] **768px portrait**: Two-column grid works

#### Landscape Orientation
- [ ] **568px landscape (iPhone SE)**: Modal still accessible
- [ ] **667px landscape**: Gallery adapts to wider layout
- [ ] **1024px landscape (iPad)**: Three columns display

**Expected**: Feature works in both portrait and landscape

---

### T116.8: Performance on Mobile (P1)

#### Load Times
- [ ] **320px**: Gallery with 6 discounts loads < 3 seconds
- [ ] **375px**: Modal opens instantly on tap (< 100ms)
- [ ] **375px**: Skeleton loads immediately while fetching data

#### Scroll Performance
- [ ] **320px**: Smooth scrolling through 20+ cards (60fps)
- [ ] **375px**: No jank when opening/closing modal
- [ ] **768px**: Grid reflow smooth on orientation change

**Expected**: 60fps smooth performance on modern mobile devices

---

### T116.9: Zoom & Text Scaling (P0) - WCAG 1.4.4

#### Browser Zoom (125%, 150%, 200%)
- [ ] **320px @ 125% zoom**: Layout doesn't break
- [ ] **375px @ 150% zoom**: All text remains readable
- [ ] **768px @ 200% zoom**: Modal still functional

#### iOS Text Size (Accessibility Settings)
- [ ] **Larger Text enabled**: Cards remain readable
- [ ] **Largest Accessibility Size**: Modal text doesn't overflow

**Expected**: Feature remains usable up to 200% zoom

---

### T116.10: Dark Mode on Mobile (P1)

- [ ] **320px dark**: Focus indicators visible (outline-color adjusted)
- [ ] **375px dark**: Modal readable with dark background
- [ ] **375px dark**: Button states visible (hover/press)
- [ ] **768px dark**: Card shadows visible

**Expected**: Dark mode fully functional on mobile devices

---

## Test Execution

### Manual Testing Checklist

1. **Chrome DevTools Device Emulation**
   - iPhone SE (375x667)
   - iPhone 12 Pro (390x844)
   - iPad (768x1024)
   - Galaxy Fold (280x653 folded, 653x280 unfolded)

2. **Real Device Testing**
   - iPhone SE (iOS 17+)
   - Android phone (Chrome, Firefox)
   - iPad (Safari)

3. **Accessibility Testing**
   - VoiceOver (iOS)
   - TalkBack (Android)
   - Screen reader navigation

### Automated Testing

```typescript
// Playwright mobile viewport test
test('discount gallery responsive at 320px', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 568 });
  await page.goto('/discounts');
  
  // Check no horizontal scroll
  const scrollWidth = await page.evaluate(() => document.documentElement.scrollWidth);
  const clientWidth = await page.evaluate(() => document.documentElement.clientWidth);
  expect(scrollWidth).toBe(clientWidth);
  
  // Check touch targets
  const button = page.getByRole('button', { name: /redeem now/i }).first();
  const box = await button.boundingBox();
  expect(box?.height).toBeGreaterThanOrEqual(44);
  expect(box?.width).toBeGreaterThanOrEqual(44);
});
```

---

## Success Criteria

✅ **Must Pass (P0)**:
- All layouts work at 320px minimum
- Touch targets ≥ 44px × 44px
- No horizontal scrolling
- Text readable without zoom (min 12px for labels, 16px for body)

✅ **Should Pass (P1)**:
- Smooth performance (60fps)
- Dark mode support
- Orientation changes handled
- Icons scale properly

✅ **Nice to Have (P2)**:
- Optimized for 1920px+ screens
- Advanced gesture support (swipe to dismiss)

---

## Known Issues / Limitations

1. **Galaxy Fold (280px folded)**: Below 320px minimum, may require horizontal scroll
2. **iOS Safari 15**: Viewport units (vh) may behave differently with address bar
3. **Android Chrome**: Zoom + landscape may cause modal overflow (acceptable edge case)

---

## References

- [WCAG 2.1 Success Criterion 1.4.4 (Resize text)](https://www.w3.org/WAI/WCAG21/Understanding/resize-text.html)
- [WCAG 2.1 Success Criterion 1.4.10 (Reflow)](https://www.w3.org/WAI/WCAG21/Understanding/reflow.html)
- [WCAG 2.1 Success Criterion 2.5.5 (Target Size)](https://www.w3.org/WAI/WCAG21/Understanding/target-size.html)
- [Mobile Accessibility Guidelines](https://www.w3.org/WAI/standards-guidelines/mobile/)
