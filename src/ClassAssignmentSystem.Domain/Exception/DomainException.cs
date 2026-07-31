namespace ClassAssignmentSystem.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with ID '{key}' was not found.") { }
}
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}