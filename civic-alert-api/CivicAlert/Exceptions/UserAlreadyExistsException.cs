namespace CivicAlert.Exceptions;

public class UserAlreadyExistsException(string message) : Exception(message);
