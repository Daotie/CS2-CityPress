namespace CityPress.Core {
 public static class FinanceFacts {
  // Native expense array values are positive; GetTotalExpenses negates them.
  public static long MonthlyNet(long income,long expenses)=>income-expenses;
 }
}
