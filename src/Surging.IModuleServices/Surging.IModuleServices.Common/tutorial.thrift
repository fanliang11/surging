namespace netstd ThriftCore

service Calculator{
  
  i32 Add(1:i32 num1, 2:i32 num2)
  string SayHello();
}


service ThirdCalculator{
  
  i32 Add(1:i32 num1, 2:i32 num2)
  string SayHello();
}